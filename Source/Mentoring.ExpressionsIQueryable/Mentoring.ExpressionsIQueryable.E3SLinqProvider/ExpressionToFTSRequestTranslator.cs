using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Mentoring.ExpressionsIQueryable.E3SLinqProvider
{
    public class ExpressionToFTSRequestTranslator : ExpressionVisitor
    {
        StringBuilder resultString;
        delegate void ConstuctEqualRequest(Expression memberVal, Expression constVal);

        public string Translate(Expression exp)
        {
            resultString = new StringBuilder();
            Visit(exp);

            return resultString.ToString();
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Method.Name)
            {
                case "Where":
                    {
                        if (node.Method.DeclaringType == typeof(Queryable))
                        {
                            var predicate = node.Arguments[1];
                            Visit(predicate);
                            return node;
                        }
                        else
                        {
                            return base.VisitMethodCall(node);
                        }
                    }
                case "StartsWith":
                case "EndsWith":
                case "Contains":
                    Visit(node.Object);
                    var constant = node.Arguments[0];
                    resultString.Append(node.Method.Name == "StartsWith" ? "(" : "(*");
                    Visit(constant);
                    resultString.Append(node.Method.Name == "EndsWith" ? ")" : "*)");
                    return node;
                default:
                    return base.VisitMethodCall(node);
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                //Support equal condition in where
                //<field/property> == <constant value> and vise versa
                case ExpressionType.Equal:

                    ConstuctEqualRequest visitConstructSearchStr = delegate (Expression memberVal, Expression constVal)
                    {
                        Visit(memberVal);
                        resultString.Append("(");
                        Visit(constVal);
                        resultString.Append(")");
                    };

                    if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                    {
                        visitConstructSearchStr(node.Left, node.Right);
                    }
                    else if (node.Left.NodeType == ExpressionType.Constant && node.Right.NodeType == ExpressionType.MemberAccess)
                    {
                        visitConstructSearchStr(node.Right, node.Left);
                    }
                    else
                    {
                        throw new NotSupportedException("One of the operands should be const and the other one should be property or field ");
                    }

                    break;

                //Support of operator AND (&&), conditions are separated by ';'
                //example: workstation:(EPRUIZHW0249);nativename:(Михаил)
                case ExpressionType.AndAlso:

                    Visit(node.Left);

                    resultString.Append(";");

                    Visit(node.Right);

                    break;

                default:
                    throw new NotSupportedException(string.Format("Operation {0} is not supported", node.NodeType));
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            resultString.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            resultString.Append(node.Value);

            return node;
        }
    }
}
