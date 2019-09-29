using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static SimpleJinja2DotNet.TextUtils;

namespace SimpleJinja2DotNet
{
    internal class Renderer
    {
        internal Renderer(string template, IEnumerable<Block> blocks)
        {
            Debug.Assert(template != null);
            Debug.Assert(blocks != null);

            _template = template;
            _blocks = blocks;
        }

        internal string Render(object globals)
        {
            var buffer = new StringBuilder();
            using (var writer = new StringWriter(buffer))
            {
                var context = new Context(writer, globals);
                Render(context);
            }

            return buffer.ToString();
        }

        private void Render(Context ctx)
            => RenderBlockList(ctx, _blocks);

        private void RenderBlockList(Context ctx, IEnumerable<Block> blocks)
        {
            foreach (var b in blocks)
                RenderBlock(ctx, b);
        }

        private void RenderBlock(Context ctx, Block block)
        {
            switch (block)
            {
                case TextBlock b:
                    RenderText(ctx, b);
                    break;

                case IfStatementBlock b:
                    RenderIfStatement(ctx, b);
                    break;

                case ForStatementBlock b:
                    RenderForStatement(ctx, b);
                    break;

                case ExpressionBlock b:
                    RenderExpression(ctx, b);
                    break;

                default:
                    throw new InvalidOperationException("Unknown block type to render");
            }
        }

        private void RenderText(Context ctx, TextBlock b)
        {
            ctx.Writer.Write(b.Content);
        }

        private void RenderIfStatement(Context ctx, IfStatementBlock b)
        {
            foreach (var t in b.TestList)
            {
                if (t.Test != null)
                {
                    var v = Evaluate(ctx, t.Test);
                    if (v.BooleanValue)
                    {
                        RenderBlockList(ctx, t.Body);
                        break;
                    }
                }
                else  // t.Test == null, the "else" branch
                {
                    RenderBlockList(ctx, t.Body);
                }
            }
        }

        private void RenderForStatement(Context ctx, ForStatementBlock b)
        {
            var loopVariable = (SymbolExpression)b.LoopVariable;
            var loopVariableName = loopVariable.Symbol;
            var hasShadowedVariable = ctx.Locals.ContainsKey(loopVariableName);
            var shadowedVariable = hasShadowedVariable ?
                ctx.Locals[loopVariableName] : null;

            var iterator = Evaluate(ctx, b.Iterator);

            if (iterator.ValueType != ValueType.Object
                || !(iterator.ObjectValue is IEnumerable))
                return;

            var enumerator = ((IEnumerable)iterator.ObjectValue).GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (ctx.Locals.ContainsKey(loopVariableName))
                    ctx.Locals[loopVariableName] = enumerator.Current;
                else
                    ctx.Locals.Add(loopVariableName, enumerator.Current);

                RenderBlockList(ctx, b.Body);
            }

            if (hasShadowedVariable)
                ctx.Locals[loopVariableName] = shadowedVariable;
            else
                ctx.Locals.Remove(loopVariableName);
        }

        private void RenderExpression(Context ctx, ExpressionBlock b)
        {
            var v = Evaluate(ctx, b.Expression);
            ctx.Writer.Write(v.StringValue);
        }

        private ExpressionValue Evaluate(Context ctx, Expression expression)
        {
            switch (expression)
            {
                case UnaryExpression e:
                    return EvaluateUnary(ctx, e);

                case BinaryExpression e:
                    return EvaluateBinary(ctx, e);

                case ParenthesisExpression e:
                    return EvaluateParenthesis(ctx, e);

                case LiteralExpression e:
                    return EvaluateLiteral(ctx, e);

                case SymbolExpression e:
                    return EvaluateSymbol(ctx, e);

                default:
                    throw new InvalidOperationException("Unknown expression to evaluate");
            }
        }

        private ExpressionValue EvaluateUnary(Context ctx, UnaryExpression e)
        {
            switch (e.Operator)
            {
                case UnaryOperator.Negative:
                    return EvaluateNegative(ctx, e.Expression);

                case UnaryOperator.Positive:
                    return EvaluatePositive(ctx, e.Expression);

                case UnaryOperator.Not:
                    return EvaluateNot(ctx, e.Expression);

                default:
                    throw new InvalidOperationException($"Unknown unary operator: {e.Operator}");
            }
        }

        private ExpressionValue EvaluateBinary(Context ctx, BinaryExpression e)
        {
            switch (e.Operator)
            {
                case BinaryOperator.Pipe:
                    return EvaluatePipe(ctx, e.Left, e.Right);

                case BinaryOperator.Subscript:
                    return EvaluateSubscript(ctx, e.Left, e.Right);

                case BinaryOperator.MemberAccess:
                    return EvaluateMemberAccess(ctx, e.Left, e.Right);

                case BinaryOperator.Or:
                    return EvaluateOr(ctx, e.Left, e.Right);

                case BinaryOperator.And:
                    return EvaluateAnd(ctx, e.Left, e.Right);

                case BinaryOperator.Add:
                    return EvaluateAdd(ctx, e.Left, e.Right);

                case BinaryOperator.Substract:
                    return EvaluateSubstract(ctx, e.Left, e.Right);

                case BinaryOperator.Multiply:
                    return EvaluateMultiply(ctx, e.Left, e.Right);

                case BinaryOperator.DivideFloat:
                    return EvaluateDivideFloat(ctx, e.Left, e.Right);

                case BinaryOperator.DivideInteger:
                    return EvaluateDivideInteger(ctx, e.Left, e.Right);

                case BinaryOperator.Modulo:
                    return EvaluateModulo(ctx, e.Left, e.Right);

                case BinaryOperator.Less:
                    return EvaluateLess(ctx, e.Left, e.Right);

                case BinaryOperator.LessOrEqual:
                    return EvaluateLessOrEqual(ctx, e.Left, e.Right);

                case BinaryOperator.Equal:
                    return EvaluateEqual(ctx, e.Left, e.Right);

                case BinaryOperator.GreaterOrEqual:
                    return EvaluateGreaterOrEqual(ctx, e.Left, e.Right);

                case BinaryOperator.Greater:
                    return EvaluateGreater(ctx, e.Left, e.Right);

                case BinaryOperator.NotEqual:
                    return EvaluateNotEqual(ctx, e.Left, e.Right);

                default:
                    throw new InvalidOperationException($"Unknown binary operator: {e.Operator}");
            }
        }

        private ExpressionValue EvaluateParenthesis(Context ctx, ParenthesisExpression e)
        {
            return Evaluate(ctx, e.Expression);
        }

        private ExpressionValue EvaluateSymbol(Context ctx, SymbolExpression e)
        {
            if (ctx.Locals.ContainsKey(e.Symbol))
                return AsValue(ctx.Locals[e.Symbol]);
            else if (ContainsValue(ctx.Globals, e.Symbol))
                return GetValue(ctx.Globals, e.Symbol);
            else
                return ExpressionValue.Empty;
        }

        private ExpressionValue EvaluateLiteral(Context ctx, LiteralExpression e)
        {
            switch (e.ValueType)
            {
                case ValueType.String:
                    return new ExpressionValue(e.StringValue);

                case ValueType.Integer:
                    return new ExpressionValue(e.IntegerValue);

                case ValueType.Float:
                    return new ExpressionValue(e.FloatValue);

                case ValueType.Boolean:
                    return new ExpressionValue(e.BooleanValue);

                default:
                    throw new InvalidOperationException("Unknown literal type to evaluate");
            }
        }

        private ExpressionValue EvaluateNegative(Context ctx, Expression e)
        {
            var v = Evaluate(ctx, e);
            switch (v.ValueType)
            {
                case ValueType.Integer:
                    return new ExpressionValue(-v.IntegerValue);
                case ValueType.Float:
                    return new ExpressionValue(-v.FloatValue);
                default:
                    throw ThrowHelper(e, RenderingError.UnsupportedOperation);
            }
        }

        private ExpressionValue EvaluatePositive(Context ctx, Expression e)
        {
            var v = Evaluate(ctx, e);
            switch (v.ValueType)
            {
                case ValueType.Integer:
                case ValueType.Float:
                    return v;
                default:
                    throw ThrowHelper(e, RenderingError.UnsupportedOperation);
            }
        }

        private ExpressionValue EvaluateNot(Context ctx, Expression e)
        {
            var v = Evaluate(ctx, e);
            return new ExpressionValue(!v.BooleanValue);
        }

        private ExpressionValue EvaluatePipe(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var argList = new List<object>();
            argList.Add(vl.ObjectValue);
            string filterSymbol = null;

            switch (r)
            {
                case SymbolExpression e:
                    filterSymbol = e.Symbol;
                    break;

                case BinaryExpression e
                when e.Operator == BinaryOperator.FunctionCall
                && e.Left is SymbolExpression
                && e.Right is ListExpression:
                    filterSymbol = ((SymbolExpression)e.Left).Symbol;
                    var args = ((ListExpression)e.Right).Expressions
                        .Select(a => Evaluate(ctx, a).ObjectValue);
                    argList.AddRange(args);
                    break;

                default:
                    throw new InvalidOperationException("Invalid filter syntax");
            }

            if (!ctx.Filters.ContainsKey(filterSymbol))
                throw ThrowHelper(r, RenderingError.UnsupportedFilter);

            try
            {
                var result = ctx.Filters[filterSymbol].Invoke(null, argList.ToArray());
                return AsValue(result);
            }
            catch (Exception ex)
            {
                throw ThrowHelper(r, RenderingError.FilterCallFailed, ex);
            }
        }

        private ExpressionValue EvaluateSubscript(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            switch (vl.ObjectValue)
            {
                case IList list:
                    if (vr.IntegerValue >= 0 && vr.IntegerValue < list.Count)
                        return AsValue(list[(int)vr.IntegerValue]);
                    else
                        return ExpressionValue.Empty;

                case IDictionary dict:
                    if (dict.Contains(vr.ObjectValue))
                        return AsValue(dict[vr.ObjectValue]);
                    else
                        return ExpressionValue.Empty;

                default:
                    return ExpressionValue.Empty;
            }
        }

        private ExpressionValue EvaluateMemberAccess(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);

            switch (r)
            {
                case SymbolExpression e:
                    return GetValue(vl.ObjectValue, e.Symbol);

                default:
                    throw new InvalidOperationException("Member is not a symbol");
            }
        }

        private ExpressionValue EvaluateOr(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            if (vl.BooleanValue)
                return new ExpressionValue(true);

            var vr = Evaluate(ctx, r);
            return new ExpressionValue(vr.BooleanValue);
        }

        private ExpressionValue EvaluateAnd(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            if (!vl.BooleanValue)
                return new ExpressionValue(false);

            var vr = Evaluate(ctx, r);
            return new ExpressionValue(vr.BooleanValue);
        }

        private ExpressionValue EvaluateAdd(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    // string + <any> => string
                    return new ExpressionValue(vl.StringValue + vr.StringValue);

                case ValueType.Integer:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // int + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        case ValueType.Integer:
                            // int + int => int
                            return new ExpressionValue(vl.IntegerValue + vr.IntegerValue);
                        case ValueType.Float:
                            // int + float => float
                            return new ExpressionValue(vl.FloatValue + vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // float + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        case ValueType.Integer:
                        case ValueType.Float:
                            // float + int/float => float
                            return new ExpressionValue(vl.FloatValue + vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Boolean:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // bool + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        default:
                            throw error();
                    }

                case ValueType.Object:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            // obj + string => string
                            return new ExpressionValue(vl.StringValue + vr.StringValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateSubstract(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.Integer:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                            // int - int => int
                            return new ExpressionValue(vl.IntegerValue - vr.IntegerValue);
                        case ValueType.Float:
                            // int - float -> float
                            return new ExpressionValue(vl.FloatValue - vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            // float - int/float => float
                            return new ExpressionValue(vl.FloatValue - vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateMultiply(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.Integer:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                            // int * int => int
                            return new ExpressionValue(vl.IntegerValue * vr.IntegerValue);
                        case ValueType.Float:
                            // int * float -> float
                            return new ExpressionValue(vl.FloatValue * vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            // float * int/float => float
                            return new ExpressionValue(vl.FloatValue * vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateDivideFloat(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            if ((vl.ValueType == ValueType.Integer || vl.ValueType == ValueType.Float)
                && (vr.ValueType == ValueType.Integer || vr.ValueType == ValueType.Float))
            {
                if (vr.FloatValue == 0)
                    throw ThrowHelper(r, RenderingError.DividedByZero);
                else
                    return new ExpressionValue(vl.FloatValue / vr.FloatValue);
            }
            else
            {
                throw ThrowHelper(l, RenderingError.UnsupportedOperation);
            }
        }

        private ExpressionValue EvaluateDivideInteger(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            if (vl.ValueType == ValueType.Integer && vr.ValueType == ValueType.Integer)
            {
                if (vr.IntegerValue == 0)
                    throw ThrowHelper(r, RenderingError.DividedByZero);
                else
                    return new ExpressionValue(vl.IntegerValue / vr.IntegerValue);
            }
            else
            {
                throw ThrowHelper(l, RenderingError.UnsupportedOperation);
            }
        }

        private ExpressionValue EvaluateModulo(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            if (vl.ValueType == ValueType.Integer && vr.ValueType == ValueType.Integer)
            {
                if (vr.IntegerValue == 0)
                    throw ThrowHelper(r, RenderingError.DividedByZero);
                else
                    return new ExpressionValue(vl.IntegerValue % vr.IntegerValue);
            }
            else
            {
                throw ThrowHelper(l, RenderingError.UnsupportedOperation);
            }
        }

        private ExpressionValue EvaluateLess(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) < 0);
                        default:
                            throw error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue < vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateLessOrEqual(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) <= 0);
                        default:
                            throw error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue <= vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateEqual(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) == 0);
                        default:
                            throw error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue == vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Boolean:
                    switch (vr.ValueType)
                    {
                        case ValueType.Boolean:
                            return new ExpressionValue(vl.BooleanValue == vr.BooleanValue);
                        default:
                            throw error();
                    }

                case ValueType.Object:
                    return new ExpressionValue(vl.ObjectValue == vr.ObjectValue);

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateGreaterOrEqual(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) >= 0);
                        default:
                            throw error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue >= vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateGreater(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) > 0);
                        default:
                            throw error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue > vr.FloatValue);
                        default:
                            throw error();
                    }

                default:
                    throw error();
            }
        }

        private ExpressionValue EvaluateNotEqual(Context ctx, Expression l, Expression r)
        {
            var vl = Evaluate(ctx, l);
            var vr = Evaluate(ctx, r);

            Exception error() =>
                ThrowHelper(l, RenderingError.UnsupportedOperation);

            switch (vl.ValueType)
            {
                case ValueType.String:
                    switch (vr.ValueType)
                    {
                        case ValueType.String:
                            return new ExpressionValue(string.Compare(vl.StringValue, vr.StringValue) != 0);
                        default:
                            throw error();
                    }

                case ValueType.Integer:
                case ValueType.Float:
                    switch (vr.ValueType)
                    {
                        case ValueType.Integer:
                        case ValueType.Float:
                            return new ExpressionValue(vl.FloatValue != vr.FloatValue);
                        default:
                            throw error();
                    }

                case ValueType.Boolean:
                    switch (vr.ValueType)
                    {
                        case ValueType.Boolean:
                            return new ExpressionValue(vl.BooleanValue != vr.BooleanValue);
                        default:
                            throw error();
                    }

                case ValueType.Object:
                    return new ExpressionValue(vl.ObjectValue != vr.ObjectValue);

                default:
                    throw error();
            }
        }

        private static ExpressionValue AsValue(object data)
        {
            var ot = data == null ? typeof(object) : data.GetType();

            if (ot == typeof(string))
                return new ExpressionValue((string)data);
            else if (ot == typeof(int))
                return new ExpressionValue((int)data);
            else if (ot == typeof(long))
                return new ExpressionValue((long)data);
            else if (ot == typeof(float))
                return new ExpressionValue((float)data);
            else if (ot == typeof(double))
                return new ExpressionValue((double)data);
            else if (ot == typeof(bool))
                return new ExpressionValue((bool)data);
            else
                return new ExpressionValue(data);
        }

        private static bool ContainsValue(object data, string symbol)
        {
            if (data == null)
                return false;

            var t = data.GetType();
            return t.GetProperty(symbol) != null
                || t.GetField(symbol) != null;
        }

        private static ExpressionValue GetValue(object data, string symbol)
        {
            if (data != null)
            {
                var t = data.GetType();

                var p = t.GetProperty(symbol);
                if (p != null)
                    return AsValue(p.GetValue(data));

                var f = t.GetField(symbol);
                if (f != null)
                    return AsValue(f.GetValue(data));
            }

            return ExpressionValue.Empty;
        }

        private RenderingException ThrowHelper(SyntaxNode n, RenderingError e,
            Exception innerException = null)
        {
            (var r, var c) = GetCharPosition(_template, n.Start);
            return new RenderingException(n.Start, r, c, e, innerException);
        }

        private readonly string _template;
        private readonly IEnumerable<Block> _blocks;

        private class Context
        {
            internal Context(TextWriter writer, object globals)
            {
                Writer = writer;
                Globals = globals;
                Locals = new Dictionary<string, object>();

                Filters = PredefinedFilters;
            }

            internal TextWriter Writer { get; }

            internal object Globals { get; }

            internal IDictionary<string, object> Locals { get; }

            internal IDictionary<string, MethodInfo> Filters { get; }

            private static readonly IDictionary<string, MethodInfo> PredefinedFilters
                = new Dictionary<string, MethodInfo>
                {
                    { "replace",  typeof(Filters).GetMethod("Replace", BindingFlags.NonPublic | BindingFlags.Static)}
                };
        }

        private class Filters
        {
            internal static string Replace(string str, string oldValue, string newValue)
            {
                return str.Replace(oldValue, newValue);
            }
        }

        private struct ExpressionValue
        {
            internal static readonly ExpressionValue Empty
                = new ExpressionValue("");

            internal ExpressionValue(string stringValue)
            {
                ValueType = ValueType.String;
                StringValue = stringValue;
                long.TryParse(stringValue, out var integerValue);
                IntegerValue = integerValue;
                double.TryParse(stringValue, out var floatValue);
                FloatValue = floatValue;
                BooleanValue = !string.IsNullOrEmpty(stringValue);
                ObjectValue = stringValue;
            }

            internal ExpressionValue(long integerValue)
            {
                ValueType = ValueType.Integer;
                StringValue = integerValue.ToString();
                IntegerValue = integerValue;
                FloatValue = integerValue;
                BooleanValue = integerValue != 0;
                ObjectValue = integerValue;
            }

            internal ExpressionValue(double floatValue)
            {
                ValueType = ValueType.Float;
                StringValue = floatValue.ToString();
                IntegerValue = (long)floatValue;
                FloatValue = floatValue;
                BooleanValue = floatValue != 0.0;
                ObjectValue = floatValue;
            }

            internal ExpressionValue(bool booleanValue)
            {
                ValueType = ValueType.Boolean;
                StringValue = booleanValue.ToString();
                IntegerValue = booleanValue ? 1 : 0;
                FloatValue = booleanValue ? 1.0 : 0.0;
                BooleanValue = booleanValue;
                ObjectValue = booleanValue;
            }

            internal ExpressionValue(object objectValue)
            {
                ValueType = ValueType.Object;
                var stringValue = objectValue != null ? objectValue.ToString() : "";
                StringValue = stringValue;
                long.TryParse(stringValue, out var integerValue);
                IntegerValue = integerValue;
                double.TryParse(stringValue, out var floatValue);
                FloatValue = floatValue;
                BooleanValue = !string.IsNullOrEmpty(stringValue);
                ObjectValue = objectValue;
            }

            internal ValueType ValueType { get; }

            internal string StringValue { get; }

            internal long IntegerValue { get; }

            internal double FloatValue { get; }

            internal bool BooleanValue { get; }

            internal object ObjectValue { get; }
        }
    }
}
