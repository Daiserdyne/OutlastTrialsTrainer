using System.Collections.Frozen;
using OutlastTrialsTrainer.Trainer;
using ReadWriteMemory;
using ReadWriteMemory.Entities;
using ReadWriteMemory.Interfaces;
using ReadWriteMemory.Services;
using ReadWriteMemory.Utilities;

namespace OutlastTrialsTrainer;

public sealed class OutlastTrialsTrainer : IDisposable
{
     private readonly RwMemory _memory = 
         RwMemoryHelper.CreateAndGetSingletonInstance("TOTClient-Win64-Shipping");

    private readonly FrozenDictionary<string, IMemoryTrainer> _implementedTrainer =
        new Dictionary<string, IMemoryTrainer>
        {
            {
                nameof(Freecam), new Freecam()
            }
        }.ToFrozenDictionary();

    private bool _freecamEnabled;

    public async Task Main(CancellationToken cancellationToken)
    {
        _memory.OnProcessStateChanged += MemoryOnProcessOnStateChanged;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await HandleTrainerTree(cancellationToken);

            await Task.Delay(1, cancellationToken);
        }
    }

    private async Task HandleTrainerTree(CancellationToken cancellationToken)
    {
        while (_freecamEnabled)
        {
            await HandleFreecam();
            await Task.Delay(1, cancellationToken);
        }

        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_F4))
        {
            _freecamEnabled = true;
            await _implementedTrainer[nameof(Freecam)].Enable("enable_freecam");
        }
    }

    private async Task HandleFreecam()
    {
        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_F4))
        {
            _freecamEnabled = false;

            await _implementedTrainer[nameof(Freecam)].Disable();

            return;
        }

        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_W, false))
        {
            await _implementedTrainer[nameof(Freecam)].Enable("forward");
        }

        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_S, false))
        {
            await _implementedTrainer[nameof(Freecam)].Enable("backward");
        }

        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_E, false))
        {
            await _implementedTrainer[nameof(Freecam)].Enable("up");
        }

        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_Q, false))
        {
            await _implementedTrainer[nameof(Freecam)].Enable("down");
        }

        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_A, false))
        {
            await _implementedTrainer[nameof(Freecam)].Enable("left");
        }

        if (await Hotkeys.KeyPressedAsync(Hotkeys.Key.VK_D, false))
        {
            await _implementedTrainer[nameof(Freecam)].Enable("right");
        }
    }

    private static void MemoryOnProcessOnStateChanged(ProgramState newState)
    {
        Console.WriteLine($"Process has been {newState}.");
    }

    public void Dispose()
    {
        _memory.Dispose();
    }
}