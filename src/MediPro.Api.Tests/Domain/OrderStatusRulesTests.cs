using FluentAssertions;
using MediPro.Api.Domain;
using Xunit;

namespace MediPro.Api.Tests.Domain;

public class OrderStatusRulesTests
{
    [Theory]
    [InlineData(OrderStatus.Submitted, OrderStatus.Submitted)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Processing, OrderStatus.Processing)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Delivered)]
    public void CanTransition_SameStatus_ReturnsTrue(OrderStatus status, OrderStatus targetStatus)
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(status, targetStatus);

        // Assert
        result.Should().BeTrue("same status transitions are always allowed (idempotent)");
    }

    [Theory]
    [InlineData(OrderStatus.Submitted, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Submitted, OrderStatus.OnHold)]
    [InlineData(OrderStatus.Submitted, OrderStatus.Rejected)]
    [InlineData(OrderStatus.Submitted, OrderStatus.Cancelled)]
    public void CanTransition_FromSubmitted_ValidTransitions_ReturnsTrue(OrderStatus from, OrderStatus to)
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(from, to);

        // Assert
        result.Should().BeTrue($"transition from {from} to {to} should be allowed");
    }

    [Theory]
    [InlineData(OrderStatus.Submitted, OrderStatus.Processing)]
    [InlineData(OrderStatus.Submitted, OrderStatus.Dispatched)]
    [InlineData(OrderStatus.Submitted, OrderStatus.Delivered)]
    public void CanTransition_FromSubmitted_InvalidTransitions_ReturnsFalse(OrderStatus from, OrderStatus to)
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(from, to);

        // Assert
        result.Should().BeFalse($"transition from {from} to {to} should NOT be allowed");
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Processing)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.OnHold)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    public void CanTransition_FromConfirmed_ValidTransitions_ReturnsTrue(OrderStatus from, OrderStatus to)
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(from, to);

        // Assert
        result.Should().BeTrue($"transition from {from} to {to} should be allowed");
    }

    [Theory]
    [InlineData(OrderStatus.Processing, OrderStatus.Dispatched)]
    [InlineData(OrderStatus.Processing, OrderStatus.OnHold)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled)]
    public void CanTransition_FromProcessing_ValidTransitions_ReturnsTrue(OrderStatus from, OrderStatus to)
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(from, to);

        // Assert
        result.Should().BeTrue($"transition from {from} to {to} should be allowed");
    }

    [Theory]
    [InlineData(OrderStatus.Dispatched, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Dispatched, OrderStatus.Cancelled)]
    public void CanTransition_FromDispatched_ValidTransitions_ReturnsTrue(OrderStatus from, OrderStatus to)
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(from, to);

        // Assert
        result.Should().BeTrue($"transition from {from} to {to} should be allowed");
    }

    [Theory]
    [InlineData(OrderStatus.Rejected, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Rejected, OrderStatus.Processing)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Processing)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processing)]
    public void CanTransition_FromTerminalStatus_ReturnsFalse(OrderStatus from, OrderStatus to)
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(from, to);

        // Assert
        result.Should().BeFalse($"terminal status {from} should not allow transition to {to}");
    }

    [Fact]
    public void AllowedTargetsSummary_SubmittedStatus_ReturnsValidTargets()
    {
        // Arrange & Act
        var summary = OrderStatusRules.AllowedTargetsSummary(OrderStatus.Submitted);

        // Assert
        summary.Should().Contain("Confirmed");
        summary.Should().Contain("OnHold");
        summary.Should().Contain("Rejected");
        summary.Should().Contain("Cancelled");
    }

    [Fact]
    public void AllowedTargetsSummary_TerminalStatus_ReturnsNone()
    {
        // Arrange & Act
        var summary = OrderStatusRules.AllowedTargetsSummary(OrderStatus.Delivered);

        // Assert
        summary.Should().Be("none (terminal)");
    }

    [Fact]
    public void CanTransition_OnHoldToConfirmed_ReturnsTrue()
    {
        // Arrange & Act
        var result = OrderStatusRules.CanTransition(OrderStatus.OnHold, OrderStatus.Confirmed);

        // Assert
        result.Should().BeTrue("orders on hold should be able to return to confirmed");
    }
}
