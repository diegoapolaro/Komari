using Komari.Domain.Enums;

namespace Komari.Application.Tables.DTOs;

public record UpdateTableStatusRequest(
    TableStatus Status
);
