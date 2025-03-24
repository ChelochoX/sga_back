using System.Security.Cryptography;
using System.Text;

namespace sga_back.Wrappers;

public class PasswordGenerator
{
    public static string GenerarContrasenaTemporal(int longitud = 8)
    {
        const string caracteresPermitidos = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz123456789";
        StringBuilder contraseña = new();

        using var rng = RandomNumberGenerator.Create();
        byte[] buffer = new byte[longitud];

        rng.GetBytes(buffer);

        for (int i = 0; i < longitud; i++)
        {
            var index = buffer[i] % caracteresPermitidos.Length;
            contraseña.Append(caracteresPermitidos[index]);
        }

        return contraseña.ToString();
    }
}
