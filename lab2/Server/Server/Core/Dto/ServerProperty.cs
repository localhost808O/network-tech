namespace Server.Core.Dto;

public record ServerProperty(string Address, string Dns, int Port, int MaxConnections);