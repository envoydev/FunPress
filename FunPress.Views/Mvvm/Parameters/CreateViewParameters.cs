using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunPress.Views.Mvvm.Parameters
{
    public class CreateViewParameters
    {
        public IManageView Parent { get; set; }
        public object AdditionalParameters { get; set; }
    }
}
