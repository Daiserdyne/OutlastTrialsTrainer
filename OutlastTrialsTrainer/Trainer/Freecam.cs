using System.Numerics;
using OutlastTrialsTrainer.Utilities;
using ReadWriteMemory;
using ReadWriteMemory.Entities;
using ReadWriteMemory.Interfaces;
using ReadWriteMemory.Services;

namespace OutlastTrialsTrainer.Trainer;

public sealed class Freecam : IMemoryTrainer
{
    private readonly RwMemory _memory = RwMemoryHelper.RwMemory;

    private readonly MemoryAddress _cameraFunctionAddress =
        new(0x86A7A9, "TOTClient-Win64-Shipping.exe");

    private readonly MemoryAddress _cameraCoordinatesAddress =
        new(0x05D759E0, "TOTClient-Win64-Shipping.exe",
            0x218, 0x3A0, 0x2A0, 0x1E0);

    private readonly MemoryAddress _cameraPitchAddress =
        new(0x05E2F920, "TOTClient-Win64-Shipping.exe",
            0x30, 0x260, 0x2A0, 0x6C0, 0x68, 0x428, 0x74);

    private readonly MemoryAddress _cameraYawAddress =
        new(0x05D759E0, "TOTClient-Win64-Shipping.exe",
            0x208, 0x868, 0x20, 0x29C);

    private readonly byte[] _scriptFunction = [0x83, 0xBB, 0x34, 0x01, 0x00, 0x00, 0x00, 0x0F, 0x84, 0x0D, 0x00, 
        0x00, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0xE9, 0x08, 0x00, 0x00, 0x00, 0x44, 0x0F, 0x11, 
        0xAB, 0xE0, 0x01, 0x00, 0x00, 0xE9, 0x8A, 0xA7, 0x87, 0x00];
    
    private readonly byte[] _deactivateCameraFunction = TrainerHelper.CreateAndGetNops(8);
    private readonly byte[] _originalCameraFunctionOpcodes = [0x44, 0x0F, 0x11, 0xAB, 0xE0, 0x01, 0x00, 0x00];
    
    private Vector3 _currentCameraPosition = Vector3.Zero;
    private float _currentPitch;
    private float _currentYaw;
    
    public Freecam() => _memory.OnReinitilizeTargetProcess += OnReinitilizeTargetProcess;
    
    public int Id => 4;

    public string TrainerName => nameof(Freecam);

    public string Description => "Ingame freecam.";

    public bool DisableWhenDispose => true;

    private void RefreshYaw(float newYaw) => _currentYaw = newYaw;
    private void RefreshPitch(float newPitch) => _currentPitch = newPitch;
    
    public async Task<bool> Enable(params string[]? args)
    {
        var command = args!.First();

        switch (command)
        {
            case "enable_freecam":
            {
                var caveAddress = await _memory.CreateOrResumeDetour(_cameraFunctionAddress, _scriptFunction, 
                    8, 18);

                if (caveAddress == nuint.Zero)
                {
                    await Disable();

                    return false;
                }
                
                if (!_memory.ReadValue(_cameraCoordinatesAddress, out _currentCameraPosition)
                    && _currentCameraPosition == Vector3.Zero)
                {
                    await Disable();
                    
                    return false;
                }

                if (!_memory.ReadValueConstant<float>(_cameraPitchAddress, 
                        RefreshPitch, 
                        TimeSpan.FromMilliseconds(5)))
                {
                    await Disable();

                    return false;
                }

                if (!_memory.ReadValueConstant<float>(_cameraYawAddress, 
                        RefreshYaw, 
                        TimeSpan.FromMilliseconds(5)))
                {
                    await Disable();
                    
                    return false;
                }
                
                break;
            }
            case "forward":
            {
                var newCoordinates = TrainerHelper.TeleportForward(_currentCameraPosition, 
                    _currentYaw - 90f, _currentPitch , 40f);
                
                WriteNewCameraCoords(newCoordinates);

                break;
            }
            case "backward":
            {
                var newCoordinates = TrainerHelper.TeleportBackward(_currentCameraPosition, 
                    _currentYaw - 90f, _currentPitch, 40f);
                
                WriteNewCameraCoords(newCoordinates);
                
                break;
            }
            case "up":
            {
                _currentCameraPosition.Z += 10f;
                
                WriteNewCameraCoords(_currentCameraPosition);
                
                break;
            }
            case "down":
            {
                _currentCameraPosition.Z -= 10f;
                
                WriteNewCameraCoords(_currentCameraPosition);
                
                break;
            }
            case "right":
            {
                var newCoordinates = TrainerHelper.TeleportForwardWithoutZ(_currentCameraPosition, 
                    _currentYaw, 40f);
                
                WriteNewCameraCoords(newCoordinates);

                break;
            }
            case "left":
            {
                var newCoordinates = TrainerHelper.TeleportForwardWithoutZ(_currentCameraPosition, 
                    _currentYaw - 180f, 40f);
                
                WriteNewCameraCoords(newCoordinates);
                
                break;
            }
        }

        await Task.CompletedTask;
        
        return true;
    }

    public async Task<bool> Disable(params string[]? args)
    {
        _memory.StopReadingValueConstant(_cameraPitchAddress);
        _memory.StopReadingValueConstant(_cameraYawAddress);
        
        _memory.PauseOpenedCodeCave(_cameraFunctionAddress);
        
        await Task.CompletedTask;
        
        return true;
    }
    
    private void WriteNewCameraCoords(Vector3 newCoordinates)
    {
        _currentCameraPosition = newCoordinates;

        _memory.WriteValue(_cameraCoordinatesAddress, newCoordinates);
    }

    private void OnReinitilizeTargetProcess()
    {
        _currentCameraPosition = Vector3.Zero;
        _currentPitch = 0f;
        _currentYaw = 0f;
        
        _memory.StopReadingValueConstant(_cameraPitchAddress);
        _memory.StopReadingValueConstant(_cameraYawAddress);
    }
}