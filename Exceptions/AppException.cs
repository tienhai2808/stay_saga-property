namespace PropertyService.Exceptions;

public class AppException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class BadRequestException(string message) : AppException(message, 400);

public class NotFoundException(string message) : AppException(message, 404);

public class ConflictException(string message) : AppException(message, 409);

public class UnauthorizedException(string message) : AppException(message, 401);

public class ForbiddenException(string message) : AppException(message, 403);

public class ExternalServiceException(string message) : AppException(message, 502);