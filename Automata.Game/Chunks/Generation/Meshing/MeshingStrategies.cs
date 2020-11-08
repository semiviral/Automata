using System.Collections.Generic;
using Automata.Engine;
using Serilog;

namespace Automata.Game.Chunks.Generation.Meshing
{
    public class MeshingStrategies
    {
        private readonly Dictionary<string, int> _MeshingStrategiesIndexer;
        private readonly List<IMeshingStrategy> _MeshingStrategies;

        public IMeshingStrategy this[int strategyIndex] => _MeshingStrategies[strategyIndex];

        public IMeshingStrategy this[string identifier]
        {
            get => _MeshingStrategies[_MeshingStrategiesIndexer[identifier]];
            set
            {
                if (_MeshingStrategiesIndexer.ContainsKey(identifier))
                {
                    _MeshingStrategies.RemoveAt(_MeshingStrategiesIndexer[identifier]);
                    _MeshingStrategiesIndexer.Add(identifier, _MeshingStrategies.Count);
                    _MeshingStrategies.Add(value);
                }
                else
                {
                    _MeshingStrategiesIndexer.Add(identifier, _MeshingStrategies.Count);
                    _MeshingStrategies.Add(value);
                }

                Log.Debug(string.Format(FormatHelper.DEFAULT_LOGGING, nameof(MeshingStrategies), $"Registered strategy: \"{identifier}\""));
            }
        }

        public MeshingStrategies()
        {
            _MeshingStrategiesIndexer = new Dictionary<string, int>();
            _MeshingStrategies = new List<IMeshingStrategy>();
        }

        public int GetMeshingStrategyIndex(string identifier) => _MeshingStrategiesIndexer[identifier];
    }
}
