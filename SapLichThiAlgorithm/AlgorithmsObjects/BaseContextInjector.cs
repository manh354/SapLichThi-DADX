using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SapLichThiAlgorithm.AlgorithmsObjects
{
    public abstract class BaseContextInjector
    {
        protected AlgorithmContext Context { get; set; }
        public BaseContextInjector() 
        {
            WithContext = new(this);
        }
        private BaseContextInjectorWithContext WithContext { get; set; }

        protected abstract void Run();

        public BaseContextInjectorWithContext SetContext(AlgorithmContext context) 
        { 
            Context = context;
            return WithContext;
        }

        public class BaseContextInjectorWithContext
        {
            private BaseContextInjector Injector { get; set; }
            public BaseContextInjectorWithContext(BaseContextInjector injector)
            {
                Injector = injector;
            }
            public BaseContextInjector Run()
            {
                Injector.Run();
                return Injector;
            }
        }
    }
}
