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
            try
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

v0_t = re.sub(operatorValues[0], operatorValues[1], v0_x)

x, t = sympy.symbols(f'{operatorValues[0]} {operatorValues[1]}')

if('>=' in scopes[0] or '>' in scopes[0]):
    operators.reverse()

print(operatorValues[0], operatorValues[1])
print(operators[0], operators[1])
print(scopes[0], scopes[1])
print(leftSide)
print(rightSide)
print(v0_t, v0_x)

counter = 0
vx_x = [v0_x]
vx_t = [v0_t]
while counter != 3:
    vx_part2 = sympy.integrate('((' + operators[0] + ')*(' + vx_t[counter] + '))', (t, x, rightSide))
    vx_part1 = sympy.integrate('((' + operators[1] + ')*(' + vx_t[counter] + '))', (t, leftSide, x))        

    
    print(counter, vx_part1)
    print('________________________________________')
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
            catch (Exception exception)
            {
                _logger.LogInformation($"ERROR in getVm method: {exception.Message}");
                PythonEngine.EndAllowThreads(_threadState);
                return null;
            }
        }

        public object GetCm(object vm, object leftSide, object rightSide)
        {
            try
            {
                _threadState = PythonEngine.BeginAllowThreads();
                _logger.LogInformation("*** Start GetCm method... ***");
                var cm = new object();

                using (Py.GIL())
                {
                    using (var scope = Py.CreateScope())
                    {
                        scope.Import("numpy");
                        scope.Import("sympy");
                        scope.Import("scipy");
                        scope.Import("math");
                        scope.Set("v_m", vm.ToPython());
                        scope.Set("leftSide", leftSide.ToPython());
                        scope.Set("rightSide", rightSide.ToPython());
                        scope.Exec(@"
current_function = ''

def f(x):
    return float(eval(current_function, {'cos': math.cos, 'sin':math.sin, 'tan': math.tan, 'atan':math.atan, 'x': x, 'e':math.exp}))

x = sympy.Symbol('x')
scalar_matrix = numpy.zeros((len(v_m) - 1, len(v_m) - 1))
result_matrix = numpy.zeros(len(v_m) - 1)
for i1 in range(1, len(v_m)):
    for i2 in range(1, len(v_m)):
        current_function = '((' + v_m[i2] + ')*(' + v_m[i1] + '))' 
        #result = sympy.integrate(sympy.expand(current_function), (x, float(leftSide), float(rightSide)))
        result, err = scipy.integrate.quad(f, float(leftSide), float(rightSide))
        scalar_matrix[len(v_m) - 1 - i1][len(v_m) - 1 - i2] = result

    current_function = '((' + v_m[0] + ')*(' + v_m[i1] + '))'
    #result = sympy.integrate(sympy.expand(current_function), (x, leftSide, rightSide))
    result, err = scipy.integrate.quad(f, float(leftSide), float(rightSide))
    result_matrix[len(v_m) - 1 - i1] = -result
print(scalar_matrix)
print(result_matrix)
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
            catch (Exception exception)
            {
                _logger.LogInformation($"ERROR in getCm method: {exception.Message}");
                PythonEngine.EndAllowThreads(_threadState);
                return null;
            }
        }


        public object[] GetMu(object cm)
        {
            try
            {
                _threadState = PythonEngine.BeginAllowThreads();
                _logger.LogInformation("*** Start GetMu method... ***");
                var mu = new object[3];

                using (Py.GIL())
                {
                    using (var scope = Py.CreateScope())
                    {
                        scope.Import("numpy");
                        scope.Import("clr");
                        scope.Import("System");
                        scope.Set("cm", cm.ToPython());
                        scope.Exec(@"
newCm = []
result = []
for i in range(len(cm)):
    allCoef = [coef for coef in cm[i]]
    allCoef.append(1)

    roots = numpy.polynomial.polynomial.Polynomial(allCoef).roots()
    if True in numpy.iscomplex(roots):
        continue
    else:
        result.append(roots)
        newCm.append(cm[i])      

resultForTable = [[float(j) for j in i] for i in result]
resultForTable = System.Array[System.Array[System.Double]](resultForTable)
                    ");

                        _logger.LogInformation("*** Python calculation finished... ***");
                        mu[0] = scope.Get<object>("result");
                        mu[1] = scope.Get<object>("resultForTable");
                        mu[2] = scope.Get<object>("newCm");
                    }
                }

                PythonEngine.EndAllowThreads(_threadState);
                _logger.LogInformation("*** Return mu... ***");
                return mu;
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"ERROR in getMu method: {exception.Message}");
                PythonEngine.EndAllowThreads(_threadState);
                return null;
            }
        }

        public object GetUn(object vm, object cm, object mu)
        {
            try
            {
                _threadState = PythonEngine.BeginAllowThreads();
                _logger.LogInformation("*** Start GeUn method... ***");
                dynamic un;

                using (Py.GIL())
                {
                    using (var scope = Py.CreateScope())
                    {
                        scope.Import("sympy");
                        scope.Set("vm", vm.ToPython());
                        scope.Set("cm", cm.ToPython());
                        scope.Set("mu", mu.ToPython());
                        scope.Exec(@"
def getZn(zm, zj):
    zjm = ''
    for i in range(zj):    
        zjm += '((' + str(cm[zm][i]) + str(')*(') + vm[zj-i] + '))'
        if(i != zj-1):
            zjm += '+'
    return zjm     

unh = []
for um in range(len(mu)):
    unm = []  
    for un in range(len(mu[um])):
        uij = ''
        for j in range(len(mu[um])):    
            uij += '(('+getZn(um, j+1)+')*('+str(mu[um][un])+'**'+str(j+1)+'))'
            if(j != len(mu[um])-1):
                uij += '+'            
        unm.append(uij)
    unh.append(unm)
unh.reverse()
                    ");

                        _logger.LogInformation("*** Python calculation finished... ***");
                        un = scope.Get("unh");
                    }
                }

                var result = (string[][])un;

                _logger.LogInformation("*** Return un... ***");
                PythonEngine.EndAllowThreads(_threadState);
                return result;
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"ERROR in getUn method: {exception.Message}");
                PythonEngine.EndAllowThreads(_threadState);
                return null;
            }
        }


        public object[] GetPlot(object un, object leftSide, object rightSide)
        {
            try
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
                        scope.Import("sympy");
                        scope.Import("clr");
                        scope.Import("math");
                        scope.Import("System");
                        scope.Set("un", un.ToPython());
                        scope.Set("leftSide", leftSide.ToPython());
                        scope.Set("rightSide", rightSide.ToPython());
                        scope.Exec(@"
end = len(un)
x = sympy.Symbol('x')
xi = numpy.linspace(float(leftSide), float(rightSide), 100)
allGraphs = []
for uni in range(0, end):    
    uniGraphs = []
    for ui in range(0, len(un[uni])):
        def norma_f(x):
            return float(eval('('+str(un[uni][ui])+')**2', {'cos': math.cos, 'sin':math.sin, 'tan': math.tan, 'atan':math.atan, 'x': x, 'e':math.exp}))

        def f(x):
            return float(eval(un[uni][ui], {'cos': math.cos, 'sin':math.sin, 'tan': math.tan, 'atan':math.atan, 'x': x, 'e':math.exp}))

        norma, err = scipy.integrate.quad(norma_f, float(leftSide), float(rightSide)) 
        
        #print(f'Norma {uni}: ', norma)
        f_ui = [f(i)/math.sqrt(norma) for i in xi]        
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
            catch (Exception exception)
            {
                _logger.LogInformation($"ERROR in getPlot method: {exception.Message}");
                PythonEngine.EndAllowThreads(_threadState);
                return null;
            }
        }


        public object GetMainResult(object cm, object mu)
        {
            try
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
        x = sympy.Symbol('x')
        y = eval(func, {'cos': math.cos, 'sin':math.sin, 'tan': math.tan, 'atan':math.atan, 'x': x})
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
    
    if(max_val == 0):    
        return xs

    while math.fabs(xs - temp) >= math.sqrt((2*min_val*e)/max_val):  # перевірка критерію
        temp = xs
        xs = xs - (f(xs)/fn(xs))  # формула для обчислення Xi+1
    
    return xs


result = []  
for i in range(0, len(cm)):
    if(numpy.iscomplex(mu[i]).any()):        
        continue 

    polinom = ''
    for j in range(0, len(cm[i])):
        if(j != len(cm[i]) - 1):
            polinom = polinom + str(cm[i][j])+ '*x**'+ str(j)+'+'
        else:
            polinom = polinom + str(cm[i][j])+ '*x**'+ str(j)         

    concreteRoots = []
    for item in mu[i]: 
        concreteRoots.append(tangent(polinom, item-0.001, item+0.001, 0.0001, False))

    result.append(concreteRoots)
result.append([])

result = System.Array[System.Array[System.Double]](result)
                    ");

                        _logger.LogInformation("*** Python calculation finished... ***");
                        result = scope.Get<object>("result");
                    }
                }

                PythonEngine.EndAllowThreads(_threadState);
                _logger.LogInformation("*** Return mu... ***");
                return result;
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"ERROR in makeMainCaculation method: {exception.Message}");
                PythonEngine.EndAllowThreads(_threadState);
                return null;
            }
        }            
    }
}