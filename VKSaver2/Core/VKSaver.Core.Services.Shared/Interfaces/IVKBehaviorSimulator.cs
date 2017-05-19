using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    /// <summary>
    /// Позволяет симулировать поведение по запросам оффициального клиента ВКонтакте.
    /// </summary>
    public interface IVKBehaviorSimulator
    {
        bool IsSimulationComplete { get; set; }

        Task StartSimulation();
    }
}
