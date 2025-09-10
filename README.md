# Detect DirectML

## Versão 1.0.0

**Detecta suporte a DirectML no sistema**

Exemplo:

```C#
// No início da sua aplicação ou antes de criar a sessão de inferência:

if (DirectMLSupportChecker.IsSupported())
{
    Console.WriteLine("DirectML é suportado. Inicializando sessão com DML.");
    
    // Agora é seguro criar a sessão, pois sabemos que o suporte existe.
    var sessionOptions = new SessionOptions();
    sessionOptions.AppendExecutionProvider_DML(0); 
    // ... continue a carregar seu modelo e criar a InferenceSession
}
else
{
    Console.WriteLine("DirectML não é suportado. Utilizando o provedor de CPU.");
    
    // Continue com a sessão usando apenas a CPU, sem chamar AppendExecutionProvider_DML.
    var sessionOptions = new SessionOptions();
    // ...
}
```