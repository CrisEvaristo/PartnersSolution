namespace Partners.Application.Exceptions;

public sealed class PartnerNotFoundException : Exception
{
    public PartnerNotFoundException(Guid id)
        : base($"Não foi encontrada uma entidade com o ID '{id}'.")
    {
    }
}