﻿using MMSA.BLL.Services.Interfaces;
using Python.Runtime;

namespace MMSA.BLL.Services.Implementation
{
    public class CalculationService : ICalculationService
    {
        public CalculationService()
        {
            Initialize();
        }

        private static void Initialize()
        {
            string pythonDLL = @"C:\Users\Dmytro\AppData\Local\Programs\Python\Python311\python311.dll";
            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDLL);
            PythonEngine.Initialize();
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


        public object GetVm(object funV0, object GreenFunction)
        {

            var vm = new object();

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("sympy");
                    scope.Import("re");
                    scope.Set("funV0", funV0.ToPython());
                    scope.Set("GreenFunction", GreenFunction.ToPython());
                    scope.Exec(@"
operatorA = ['(((2-t)*x)/2)', '((t*(2-x))/2)']
if GreenFunction == '2':
    operatorA=['(2-t)','(2-x)']

v0_x = ''
for i in range(len(funV0)):
    if funV0[i] == '^':
        v0_x += '**'
    else:
        v0_x += funV0[i]

v0_t = re.sub('x', 't', v0_x)

x, t = sympy.symbols('x t');

counter = 0
vx_x = [v0_x]
vx_t = [v0_t]
while counter != 6:
    vx_part1 = sympy.integrate('(' + operatorA[1] + ')*(' + vx_t[counter] + ')', (t, 0.0, x))
    vx_part2 = sympy.integrate('(' + operatorA[0] + ')*(' + vx_t[counter] + ')', (t, x, 1.0))

    full_vx = str(sympy.expand(str(vx_part1) + ""+"" + str(vx_part2)))
    vx_x.append(full_vx)
    vx_t.append(re.sub('x', 't', full_vx))

    counter = counter + 1    
                    ");
                    vm = scope.Get<object>("vx_x");
                }
            }

            return vm;
        }

        public object GetCm(object vm)
        {

            var cm = new object();

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("scipy");
                    scope.Import("scipy.integrate");
                    scope.Set("v_m", vm.ToPython());
                    scope.Exec(@"
current_function = ''

def f(x):
    return float(eval(current_function))

scalar_matrix = numpy.zeros((len(v_m) - 1, len(v_m) - 1))
result_matrix = numpy.zeros(len(v_m) - 1)
for i1 in range(1, len(v_m)):
    for i2 in range(1, len(v_m)):
        current_function = '(' + v_m[i2] + ')*(' + v_m[i1] + ')' 
        result, err = scipy.integrate.quad(f, 0.0, 1.0)
        scalar_matrix[len(v_m) - 1 - i1][len(v_m) - 1 - i2] = result

    current_function = '(' + v_m[0] + ')*(' + v_m[i1] + ')'
    result, err = scipy.integrate.quad(f, 0.0, 1.0)
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
                    cm = scope.Get<object>("c_m");
                }
            }

            return cm;
        }


        public object[] GetMu(object cm)
        {
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
                    mu[0] = scope.Get<object>("result");
                    mu[1] = scope.Get<object>("resultForTable");
                    mu[2] = scope.Get<object>("check");
                }
            }

            if ((string)mu[2] == "True")
            {
                PythonEngine.Shutdown();
            }


            return mu;
        }


        public object GetUn(object vm, object cm, object mu)
        {
            var un = new object();

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
                    un = scope.Get<object>("un");                                
                }
            }

            return un;
        }


        public object GetPlot(object un, object GreenFunction)
        {
            var plot = new object();

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Import("numpy");
                    scope.Import("matplotlib");
                    scope.Import("matplotlib.pyplot");
                    scope.Import("scipy");
                    scope.Import("clr");
                    scope.Import("System");
                    scope.Import("mpld3");
                    scope.Set("un", un.ToPython());
                    scope.Set("GreenFunction", GreenFunction.ToPython());
                    scope.Exec(@"
end = 0
if GreenFunction == '1':
    end = len(un)
if GreenFunction == '2':
    end = 3

fig, ax = matplotlib.pyplot.subplots(nrows=end, figsize=(8,24))
fig.tight_layout()

for uni in range(0, end):
    xi = numpy.linspace(0, 1, 80)
    for ui in range(0, len(un[uni])):
        def f(x):
            return float(eval(un[uni][ui]))
        
        norma, err = scipy.integrate.quad(f, 0.00, 1.00)
        f_ui = [f(i)/norma for i in xi]

        tempLabel = 'U'+str(ui + 1)
        ax[uni].plot(xi, f_ui, label=tempLabel)
    ax[uni].legend()

plot = str(mpld3.fig_to_html(fig))
                    ");
                    plot = scope.Get<object>("plot");
                }
            }

            return plot;
        }
    }
}