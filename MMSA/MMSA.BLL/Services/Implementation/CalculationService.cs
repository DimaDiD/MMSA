using Microsoft.Extensions.Logging;
using MMSA.BLL.Services.Interfaces;
using Python.Runtime;
using static IronPython.Modules._ast;

namespace MMSA.BLL.Services.Implementation
{
    public class CalculationService : ICalculationService
    {
        private readonly ILogger _logger;
        private IntPtr _threadState;


        public CalculationService(ILogger<CalculationService> logger)
        {
            Initialize();            
            _logger = logger;
        }

        private static void Initialize()
        {
            string pythonDLL = @"C:\Users\Dmytro\AppData\Local\Programs\Python\Python311\python311.dll";
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDLL);
            PythonEngine.Initialize();
        }


        public object GetVm(object funV0, object operatorValues, object operators, object scopes, object leftSide, object rightSide)
        {
            _threadState = PythonEngine.BeginAllowThreads();
            _logger.LogInformation("*** Start GetVm method... ***");
            var vm = new object();

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("sympy");
                    scope.Import("re");
                    scope.Set("funV0", funV0.ToPython());
                    scope.Set("operatorValues", operatorValues.ToPython());
                    scope.Set("operators", operators.ToPython());
                    scope.Set("scopes", scopes.ToPython());
                    scope.Set("leftSide", leftSide.ToPython());
                    scope.Set("rightSide", rightSide.ToPython());
                    scope.Exec(@"
v0_x = ''
for i in range(len(funV0)):
    if funV0[i] == '^':
        v0_x += '**'
    else:
        v0_x += funV0[i]

v0_x = v0_x.replace(' ','')

#v0_t = re.sub('x', 't', v0_x)
v0_t = re.sub(operatorValues[0], operatorValues[1], v0_x)

#x, t = sympy.symbols('x t')
x, t = sympy.symbols(f'{operatorValues[0]} {operatorValues[1]}')

if('>=' in scopes[0] or '>' in scopes[0]):
    operators.reverse()

counter = 0
vx_x = [v0_x]
vx_t = [v0_t]
while counter != 6:
    vx_part2 = sympy.integrate('(' + operators[0] + ')*(' + vx_t[counter] + ')', (t, x, rightSide))
    vx_part1 = sympy.integrate('(' + operators[1] + ')*(' + vx_t[counter] + ')', (t, leftSide, x))        

    full_vx = str(sympy.expand(str(vx_part1) + '+' + str(vx_part2)))
    vx_x.append(full_vx)
    #vx_t.append(re.sub('x', 't', full_vx))
    vx_t.append(re.sub(operatorValues[0], operatorValues[1], full_vx))

    counter = counter + 1    
                    ");

                    _logger.LogInformation("*** Python calculation finished... ***");
                    vm = scope.Get<object>("vx_x");
                }
            }
            PythonEngine.EndAllowThreads(_threadState);
            _logger.LogInformation("*** Return vm... ***");
            return vm;
        }

        public object GetCm(object vm, object leftSide, object rightSide)
        {
            _threadState = PythonEngine.BeginAllowThreads();
            _logger.LogInformation("*** Start GetCm method... ***");
            var cm = new object();

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("scipy");
                    scope.Import("scipy.integrate");
                    scope.Set("v_m", vm.ToPython());
                    scope.Set("leftSide", leftSide.ToPython());
                    scope.Set("rightSide", rightSide.ToPython());
                    scope.Exec(@"
current_function = ''

def f(x):
    return float(eval(current_function))

scalar_matrix = numpy.zeros((len(v_m) - 1, len(v_m) - 1))
result_matrix = numpy.zeros(len(v_m) - 1)
for i1 in range(1, len(v_m)):
    for i2 in range(1, len(v_m)):
        current_function = '(' + v_m[i2] + ')*(' + v_m[i1] + ')' 
        result, err = scipy.integrate.quad(f, float(leftSide), float(rightSide))
        scalar_matrix[len(v_m) - 1 - i1][len(v_m) - 1 - i2] = result

    current_function = '(' + v_m[0] + ')*(' + v_m[i1] + ')'
    result, err = scipy.integrate.quad(f, float(leftSide), float(rightSide))
    result_matrix[len(v_m) - 1 - i1] = -result

c_m = []
for i in range(0, len(scalar_matrix)):
    if i == len(scalar_matrix) - 1:
        c_m.append([result_matrix[i] / scalar_matrix[i][i]])
        break
    elif i == 0:
        c_m.append(numpy.linalg.inv(scalar_matrix).dot(result_matrix))
    else:
        new_scalar_matrix = []
        new_result_matrix = []
        for i1 in range(i, len(scalar_matrix)):
            add_scalar = []
            for i2 in range(i, len(scalar_matrix)):
                add_scalar.append(scalar_matrix[i1][i2])
            new_result_matrix.append(result_matrix[i1])
            new_scalar_matrix.append(add_scalar)

        c_m.append(numpy.linalg.inv(new_scalar_matrix).dot(new_result_matrix))   
                    ");

                    _logger.LogInformation("*** Python calculation finished... ***");
                    cm = scope.Get<object>("c_m");
                }
            }
            PythonEngine.EndAllowThreads(_threadState);
            _logger.LogInformation("*** Return cm... ***");
            return cm;
        }


        public object[] GetMu(object cm)
        {
            _threadState = PythonEngine.BeginAllowThreads();
            _logger.LogInformation("*** Start GetMu method... ***");
            var mu = new object[3];

            //Initialize();
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("clr");
                    scope.Import("System");
                    scope.Set("cm", cm.ToPython());
                    scope.Exec(@"
result = []
check = 'False'
for i in cm:
    result.append(numpy.polynomial.polynomial.Polynomial(i).roots())
resultForTable = [[float(j) for j in i] for i in result]
resultForTable = System.Array[System.Array[System.Double]](resultForTable)
internalCheck = 'False'
for j in result:
    if internalCheck == 'True':
        break
    else:
        if True in numpy.iscomplex(j):
            check = 'True'
            internalCheck = 'True'
            break
                    ");

                    _logger.LogInformation("*** Python calculation finished... ***");
                    mu[0] = scope.Get<object>("result");
                    mu[1] = scope.Get<object>("resultForTable");
                    mu[2] = scope.Get<object>("check");
                }
            }

            PythonEngine.EndAllowThreads(_threadState);
            _logger.LogInformation("*** Return mu... ***");
            return mu;
        }


        public object GetUn(object vm, object cm, object mu)
        {
            _threadState = PythonEngine.BeginAllowThreads();
            _logger.LogInformation("*** Start GeUn method... ***");
            dynamic un;

            //Initialize();
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("sympy");
                    scope.Set("vm", vm.ToPython());
                    scope.Set("cm", cm.ToPython());
                    scope.Set("mu", mu.ToPython());
                    scope.Exec(@"
zn = []
for i1 in range(0, len(cm)):
    zi = ''
    for i2 in range(0, len(cm[i1])):
        zi = zi + '(' + str(cm[i1][i2]) + str(')*(') + vm[len(cm) - i2] + ')'
        if i2 != len(cm[i1]) - 1:
            zi = zi + '+'
    zn.append(zi)

un = []
for i1 in range(0, len(mu) - 1):
    ui = []
    for i2 in range(0, len(mu[i1])):
        uij = ''
        for i3 in range(0, len(zn) - 1):
            uij = uij + ('(' + str(mu[i1][i2] ** (i3 + 1)) + ')*(' + str(zn[i3]) + ')')
            uij = str(sympy.expand(uij))
            if i3 != len(zn) - 2:
                uij = uij + '+'

        ui.append(uij)
    un.append(ui)
un.reverse()
                    ");

                    _logger.LogInformation("*** Python calculation finished... ***");
                    un = scope.Get("un");                                
                }
            }

            var result = (string[][])un;

            _logger.LogInformation("*** Return un... ***");
            PythonEngine.EndAllowThreads(_threadState);
            return result;
        }


        public object[] GetPlot(object un)//, object GreenFunction)
        {
            _threadState = PythonEngine.BeginAllowThreads();
            _logger.LogInformation("*** Start GetPlot method... ***");
            var plot = new object[2];

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("scipy");
                    scope.Import("clr");
                    scope.Import("System");
                    scope.Set("un", un.ToPython());
                    scope.Exec(@"
end = len(un)

xi = numpy.linspace(0, 1, 80)
allGraphs = []
for uni in range(0, end):    
    uniGraphs = []
    for ui in range(0, len(un[uni])):
        def f(x):
            return float(eval(un[uni][ui]))
        
        norma, err = scipy.integrate.quad(f, 0.00, 1.00)
        f_ui = [f(i)/norma for i in xi]
        
        uniGraphs.append(f_ui)
    allGraphs.append(uniGraphs)

parsedXi = System.Array[System.Double](xi)
parsedAllGraphs = System.Array[System.Array[System.Array[System.Double]]](allGraphs)
                    ");

                    _logger.LogInformation("*** Creating plot finish... ***");
                    plot[0] = scope.Get<object>("parsedXi");
                    plot[1] = scope.Get<object>("parsedAllGraphs");
                }
            }

            _logger.LogInformation("*** PythonEngine shutdown... ***");
            PythonEngine.EndAllowThreads(_threadState);

            _logger.LogInformation("*** Return plot... ***");
            return plot;
        }


        public object GetMainResult(object cm, object mu)
        {
            _threadState = PythonEngine.BeginAllowThreads();
            _logger.LogInformation("*** Start GetMu method... ***");
            var result = new object();

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("clr");
                    scope.Import("sympy");
                    scope.Import("math");
                    scope.Import("scipy.integrate");
                    scope.Import("re");
                    scope.Import("System");
                    scope.Set("cm", cm.ToPython());
                    scope.Set("mu", mu.ToPython());
                    scope.Exec(@"
def tangent(function, a, b, e, check):
    add_func = ''
    for i in range(len(function)):  # формування функції для обчислень
        if function[i] == '^':
            add_func += '**'
        else:
            add_func += function[i]
    function = add_func

    def derivative(func):  # похідна функції
        x = Symbol('x')
        y = eval(func)
        res = str(y.diff(x))
        return res

    def f(x):
        return float(eval(function))  # значення функції в точці

    def fn(x):
        return float(eval(derivative(function)))  # перша похідна

    def fnn(x):
        return float(eval(derivative(derivative(function))))  # третя похідна

    def direct(n):  # напрямок з якого відбуватиметься обрахунок
        try:
            if f(n) > 0 and fnn(n) > 0:
                return True
            elif f(n) < 0 and fnn(n) < 0:
                return True
            return False
        except:
            return False

    if direct(a):  # визначення напрямку
        xs = a
        silence = 1
        step = 0.01
        border = b
    else:
        xs = b
        silence = -1
        step = -0.01
        border = a

    # мінімальне та максимальне значення аохідної на проміжку [a, b]
    min_val = math.fabs(fn(xs))
    max_val = math.fabs(fn(xs))
    i = xs
    while i*silence <= border*silence:
        if math.fabs(fn(i)) < min_val:
            min_val = math.fabs(fn(i))
        elif math.fabs(fn(i)) > max_val:
            max_val = math.fabs(fn(i))
        i += step

    xs = (a+b)/2
    temp = b

    while math.fabs(xs - temp) >= math.sqrt((2*min_val*e)/max_val):  # перевірка критерію
        temp = xs
        xs = xs - (f(xs)/fn(xs))  # формула для обчислення Xi+1
    
    return xs


def chord(function, a, b, e, check):
    add_func = ''
    for i in range(len(function)):  # формування функції для обчислень
        if function[i] == '^':
            add_func += '**'
        else:
            add_func += function[i]
    function = add_func

    def derivative(func):  # похідна функції
        x = Symbol('x')
        y = eval(func)
        res = str(y.diff(x))
        return res

    def f(x):
        return float(eval(function))  # значення функції в точці

    def fn(x):
        return float(eval(derivative(function)))  # перша похідна

    sm = 10  # для формування першої хорди
    temp = a

    fnx_max = math.fabs(fn(a))
    fnx_min = math.fabs(fn(a))
    i = a
    while b >= i:  # пошук мінімального та максимального значення похідної на проміжку [a, b]
        sm_x = math.fabs(fn(i))
        if fnx_max < sm_x:
            fnx_max = sm_x
        if fnx_min > sm_x:
            fnx_min = sm_x
        i += 0.01

    a = (a + b) / 2  # початкова віддаль

    previous = 0
    while sm >= (e*(fnx_max-fnx_min))/fnx_max:  # критерій пошуку розвя'зку
        if sm == 10:  # формування першої хорди
            sm = math.fabs(a - temp)  # |Хі+1-Хі|

        # формування графіків та пошук розв'язку
        temp = a
        a = a - (f(a) * (b - a)) / (f(b) - f(a))  # формула для знаходження Хі+1
        if(a == previous):
            break
        previous = a

        sm = math.fabs(a - temp)  # |Хі+1-Хі|
    
    return a


result = []  
for i in cm:
    if(len(i) == 1):
        continue

    polinom = ''
    for j in range(0, len(i)):
        if(j != len(i) - 1):
            polinom = polinom + str(i[j])+ '*x**'+ str(j)+'+'
        else:
            polinom = polinom + str(i[j])+ '*x**'+ str(j)
        
    fi_x = ''
    #if(methodForExactCalculation == ""simpleIteration""):
    if (True):
        for j in range(0, len(i)-1):
            last_pow = len(i) - 1
            last_koef = i[last_pow]
            sign = ''
            if(last_koef < 0):
                sign = '-'
            else:
                sign = ''

            if(j != len(i) - 2):
                fi_x = fi_x + '('+str(i[j])+ '*x**'+ str(j)+f')/({sign}1*{last_koef}*x**{last_pow-1})'+'+'
            elif(j != len(i) - 3):
                fi_x = fi_x + '('+str(i[j])+ '*x**'+ str(j)+f')/({sign}1*{last_koef}*x**{last_pow-1})'
            else:
                continue

    if((numpy.iscomplex(mu[cm.index(i)])).any()):        
        continue            

    tangentResult = []
    for item in mu[cm.index(i)]:     
        chordValue = chord(polinom, item-0.001, item+0.001, 0.0001, False)        
        tangentValue = tangent(polinom, item-0.001, item+0.001, 0.0001, False)
        tangentResult.append(tangentValue)

    result.append(tangentResult)
#result = System.Array[System.Array[System.Double]](result)
                    ");

                    _logger.LogInformation("*** Python calculation finished... ***");
                    result = scope.Get<object>("result");
                }
            }

            //if ((string)mu[2] == "True")
            //{
            //    PythonEngine.Shutdown();
            //}
            PythonEngine.EndAllowThreads(_threadState);
            _logger.LogInformation("*** Return mu... ***");
            return result;
        }
    }
}

//        public List<string> GetVm(object funV0, object GreenFunction)
//        {           

//            dynamic vm;

//            using (Py.GIL())
//            {
//                using (var scope = Py.CreateScope())
//                {
//                    scope.Import("numpy");
//                    scope.Import("sympy");
//                    scope.Import("re");
//                    scope.Set("funV0", funV0.ToPython());
//                    scope.Set("GreenFunction", GreenFunction.ToPython());
//                    scope.Exec(@"
//operatorA = ['(((2-t)*x)/2)', '((t*(2-x))/2)']
//if GreenFunction == '2':
//    operatorA=['(2-t)','(2-x)']

//v0_x = ''
//for i in range(len(funV0)):
//    if funV0[i] == '^':
//        v0_x += '**'
//    else:
//        v0_x += funV0[i]

//v0_t = re.sub('x', 't', v0_x)

//x, t = sympy.symbols('x t');

//counter = 0
//vx_x = [v0_x]
//vx_t = [v0_t]
//while counter != 6:
//    vx_part1 = sympy.integrate('(' + operatorA[1] + ')*(' + vx_t[counter] + ')', (t, 0.0, x))
//    vx_part2 = sympy.integrate('(' + operatorA[0] + ')*(' + vx_t[counter] + ')', (t, x, 1.0))

//    full_vx = str(sympy.expand(str(vx_part1) + ""+"" + str(vx_part2)))
//    vx_x.append(full_vx)
//    vx_t.append(re.sub('x', 't', full_vx))

//    counter = counter + 1    
//                    ");
//                    vm = scope.Get("vx_x");
//                }
//            }

//            var mylist = ((string[])vm).ToList<string>();

//            foreach (var item in vm)
//            {
//                Console.WriteLine(item.ToString());
//            }

//            return new List<string>();
//        }