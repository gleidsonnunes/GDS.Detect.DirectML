using System.Runtime.InteropServices;

public static class DirectMLSupportChecker
{
    // Constantes e definições nativas
    private const string D3D12_DLL = "d3d12.dll";
    private const string DIRECTML_DLL = "DirectML.dll";
    private const long S_OK = 0L;

    // GUIDs (Identificadores únicos) para as interfaces COM que queremos criar.
    // GUID de ID3D12Device
    private static readonly Guid IID_ID3D12Device = new("189819f1-1db6-4b57-be54-1821339b85f7");
    // GUID de IDMLDevice
    private static readonly Guid IID_IDMLDevice = new("64ac267a-4781-4354-a6d3-90d32b643501");

    // Enum para o nível de funcionalidade mínimo do D3D12 que o DirectML requer.
    // D3D_FEATURE_LEVEL_11_0 é um requisito comum para hardware moderno.
    private enum D3D_FEATURE_LEVEL
    {
        D3D_FEATURE_LEVEL_11_0 = 0xb000
    }

    // Flags para a criação do dispositivo DirectML. 'None' é suficiente para uma verificação.
    [Flags]
    private enum DML_CREATE_DEVICE_FLAGS
    {
        None = 0,
        Debug = 1,
    }

    // Assinaturas P/Invoke para as funções nativas
    [DllImport(D3D12_DLL, CallingConvention = CallingConvention.StdCall, PreserveSig = true)]
    private static extern long D3D12CreateDevice(
        IntPtr pAdapter,
        D3D_FEATURE_LEVEL MinimumFeatureLevel,
        ref Guid riid,
        out IntPtr ppDevice
    );

    [DllImport(DIRECTML_DLL, CallingConvention = CallingConvention.StdCall, PreserveSig = true)]
    private static extern long DMLCreateDevice(
        IntPtr d3d12Device,
        DML_CREATE_DEVICE_FLAGS flags,
        ref Guid riid,
        out IntPtr ppvDevice
    );

    /// <summary>
    /// Verifica de forma segura se o sistema atual suporta DirectML,
    /// sem usar try-catch e interagindo diretamente com as APIs nativas.
    /// Este método é seguro contra falhas de driver que poderiam travar a aplicação.
    /// </summary>
    /// <returns>True se o DirectML for suportado, caso contrário, False.</returns>
    public static bool IsSupported()
    {
        IntPtr d3d12DevicePtr = IntPtr.Zero;
        IntPtr dmlDevicePtr = IntPtr.Zero;

        try
        {
            // --- Etapa 1: Verificar o suporte ao DirectX 12 ---
            // Tentamos criar um dispositivo D3D12.
            // Passamos IntPtr.Zero para usar o adaptador de vídeo padrão.
            Guid iid_d3d12 = IID_ID3D12Device;
            long hResultD3D12 = D3D12CreateDevice(
                IntPtr.Zero,
                D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0,
                ref iid_d3d12,
                out d3d12DevicePtr
            );

            // Se o HRESULT não for S_OK (0), a criação falhou. O sistema não suporta D3D12.
            if (hResultD3D12 != S_OK)
            {
                return false;
            }

            // --- Etapa 2: Verificar o suporte ao DirectML ---
            // Se o dispositivo D3D12 foi criado, usamos seu ponteiro para criar um dispositivo DirectML.
            Guid iid_dml = IID_IDMLDevice;
            long hResultDML = DMLCreateDevice(
                d3d12DevicePtr,
                DML_CREATE_DEVICE_FLAGS.None,
                ref iid_dml,
                out dmlDevicePtr
            );

            // Se o HRESULT não for S_OK, o DirectML não é suportado, mesmo que D3D12 seja.
            if (hResultDML != S_OK)
            {
                return false;
            }

            // Se chegamos aqui, ambos os dispositivos foram criados com sucesso.
            return true;
        }
        finally
        {
            // --- Etapa 3: Limpeza ---
            // É CRÍTICO liberar os objetos COM para evitar vazamentos de memória.
            // Marshal.Release decrementa a contagem de referências do objeto COM.
            if (dmlDevicePtr != IntPtr.Zero)
            {
                _ = Marshal.Release(dmlDevicePtr);
            }
            if (d3d12DevicePtr != IntPtr.Zero)
            {
                _ = Marshal.Release(d3d12DevicePtr);
            }
        }
    }
}