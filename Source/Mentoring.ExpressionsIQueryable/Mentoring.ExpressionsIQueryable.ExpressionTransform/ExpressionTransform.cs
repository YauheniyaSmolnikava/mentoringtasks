using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mentoring.ExpressionsIQueryable.ExpressionTransform
{
    public class ExpressionTransform : ExpressionVisitor
    {
        #region Fields

        private Dictionary<string, int> paramsConstDictionary;

        #endregion

        #region Protected Methods

        //Changement to increment/decrement the following expressions: <param> + 1 / <param> - 1
        protected override Expression VisitBinary(BinaryExpression node)
        {
            ParameterExpression param = null;
            ConstantExpression constant = null;

            if (node.NodeType == ExpressionType.Add || node.NodeType == ExpressionType.Subtract)
            {
                if (node.Left.NodeType == ExpressionType.Parameter)
                {
                    param = (ParameterExpression)node.Left;
                }
                else if (node.Left.NodeType == ExpressionType.Constant)
                {
                    constant = (ConstantExpression)node.Left;
                }

                if (node.Right.NodeType == ExpressionType.Parameter)
                {
                    param = (ParameterExpression)node.Right;
                }
                else if (node.Right.NodeType == ExpressionType.Constant)
                {
                    constant = (ConstantExpression)node.Right;
                }

                if (param != null && constant != null && constant.Type == typeof(int) && (int)constant.Value == 1 && node.NodeType == ExpressionType.Add)
                {
                    return Expression.Increment(VisitParameter(param));
                }

                if (param != null && constant != null && constant.Type == typeof(int) && (int)constant.Value == 1 && node.NodeType == ExpressionType.Subtract)
                {
                    return Expression.Decrement(VisitParameter(param));
                }
            }

            return base.VisitBinary(node);
        }

        //Replacement param to const value from the paramConstDictionary
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (paramsConstDictionary.ContainsKey(node.Name))
            {
                int dict_value;

                paramsConstDictionary.TryGetValue(node.Name, out dict_value);

                return Expression.Constant(dict_value);
            }

            return base.VisitParameter(node);
        }

        //Preventment of lambda expression's params repleacement to const values
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression body = this.Visit(node.Body);

            if (body != node.Body)
            {
                return Expression.Lambda(node.Type, body, node.Parameters);
            }

            return node;
        }

        #endregion

        #region Public Methods

        //Call base VisitAndConvert and assign dictionary field
        public Expression<T> VisitAndConvert<T>(Expression<T> node, string callerName, Dictionary<string, int> dict)
        {
            paramsConstDictionary = dict;

            return base.VisitAndConvert(node, callerName);
        }

        #endregion
    }
}
