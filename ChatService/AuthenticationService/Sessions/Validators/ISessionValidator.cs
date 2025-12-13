namespace AuthenticationService.Sessions.Validators;

public interface ISessionValidator
{
    Task<User?> Validate(SessionToken token);
}