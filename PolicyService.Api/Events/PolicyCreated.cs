using System;
using MediatR;
using PolicyService.Api.Commands.Dtos;

namespace PolicyService.Api.Events;

public class PolicyCreated : INotification
{
    public string AgentLogin { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string PolicyNumber { get; set; } = "";
    public DateTime PolicyFrom { get; set; }
    public DateTime PolicyTo { get; set; }
    public PersonDto PolicyHolder { get; set; } = new();
    public decimal TotalPremium { get; set; }
}