namespace Partners.Application.Exceptions;

public sealed class DuplicateTaxNumberException : Exception
{
    public DuplicateTaxNumberException()
        : base("Já existe uma entidade com este NIF.")
    {
    }
}