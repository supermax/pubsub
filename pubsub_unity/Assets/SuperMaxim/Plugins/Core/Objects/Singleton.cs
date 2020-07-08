using System;

namespace SuperMaxim.Core.Objects
{
    public abstract class Singleton<TInterface, TImplementation> 
        where TImplementation : TInterface, new()
    {
        public static TInterface Default { get; } = new TImplementation();

        protected Singleton()
        {
        }
    }
}
