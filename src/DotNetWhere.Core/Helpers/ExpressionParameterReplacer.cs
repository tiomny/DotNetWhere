using System.Linq.Expressions;

namespace DotNetWhere.Core.Helpers;

internal class ExpressionParameterReplacer : ExpressionVisitor
{
    public ExpressionParameterReplacer(
        IList<ParameterExpression> fromParameters,
        IList<ParameterExpression> toParameters)
    {
        for (var i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
            ParameterReplacements.Add(fromParameters[i], toParameters[i]);
    }
    private readonly Dictionary<ParameterExpression, ParameterExpression> ParameterReplacements = [];
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (ParameterReplacements.TryGetValue(node, out var replacement))
            node = replacement;
        return base.VisitParameter(node);
    }
}
