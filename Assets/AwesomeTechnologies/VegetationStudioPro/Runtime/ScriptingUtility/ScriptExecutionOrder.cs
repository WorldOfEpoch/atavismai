using System;

namespace AwesomeTechnologies.Utility
{
    public class ScriptExecutionOrder : Attribute
    {
        public int Order;

        public ScriptExecutionOrder(int _order)
        {
            Order = _order;
        }
    }
}