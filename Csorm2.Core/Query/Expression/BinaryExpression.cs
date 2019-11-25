using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Csorm2.Core.Query.Expression
{
    public class BinaryExpression: ISqlExpression
    {

        private BinaryExpression(ISqlExpression left, ISqlExpression right, string operatorSymbol)
        {
            _left = left;
            _right = right;
            _operatorSymbol = operatorSymbol;
        }

        public IEnumerable<(DbType, string, object)> GetParameters()
        {
            return _left.GetParameters().Concat(_right.GetParameters());
        }

        private readonly ISqlExpression _right;
        private readonly ISqlExpression _left;
        private readonly string _operatorSymbol;
        
        public string AsSqlString()
        {
            return _left.AsSqlString() +" "+ _operatorSymbol +" "+ _right.AsSqlString();
        }

        public static BinaryExpression Eq(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, "=");
        }
        
        public static BinaryExpression Neq(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, "<>");
        }
        
        public static BinaryExpression LT(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, "<");
        }
        
        public static BinaryExpression GT(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, ">");
        }
        
        public static BinaryExpression LTEq(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, "<=");
        }

        public static BinaryExpression GTEq(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, ">=");
        }
        
        public static BinaryExpression And(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, "AND");
        }
        public static BinaryExpression Or(ISqlExpression left, ISqlExpression right)
        {
            return new BinaryExpression(left, right, "OR");
        }
        
        
    }
}