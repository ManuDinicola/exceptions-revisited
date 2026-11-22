using System.Reflection;
using System.Runtime.CompilerServices;
using V3 = Orders.Unions;
using V3b = Orders.Unions.Handwritten;

namespace Orders.Tests;

/// <summary>
/// Backs the article's claim that the <c>union</c> keyword lowers to the same shape as the hand-written
/// version: a value type marked <see cref="UnionAttribute"/> that implements <see cref="IUnion"/>.
/// </summary>
public sealed class UnionShapeTests
{
    [Theory]
    [InlineData(typeof(V3.PlaceOrderResult))]
    [InlineData(typeof(V3b.PlaceOrderResult))]
    public void Union_IsAStructMarkedWithUnionAttribute(Type union)
    {
        Assert.True(union.IsValueType);
        Assert.NotNull(union.GetCustomAttribute<UnionAttribute>());
        Assert.True(typeof(IUnion).IsAssignableFrom(union));
    }

    [Theory]
    [InlineData(typeof(V3.PlaceOrderResult))]
    [InlineData(typeof(V3b.PlaceOrderResult))]
    public void Union_HasOneConstructorPerCase(Type union)
    {
        var caseTypes = union.GetConstructors()
            .Select(c => c.GetParameters())
            .Where(p => p.Length == 1)
            .Select(p => p[0].ParameterType.Name)
            .Order();

        Assert.Equal(
            ["CreditLimitExceeded", "CustomerBlocked", "InvalidQuantity", "OrderConfirmation",
             "OutOfStock", "UnknownCustomer", "UnknownProduct"],
            caseTypes);
    }
}
