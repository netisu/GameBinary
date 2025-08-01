
namespace Netisu
{
    public class RuntimeErrors
    {
        public static Datamodels.Instance ScriptRuntimeException(string message)
        {
            throw new MoonSharp.Interpreter.ScriptRuntimeException(message);
        }
    }
}
