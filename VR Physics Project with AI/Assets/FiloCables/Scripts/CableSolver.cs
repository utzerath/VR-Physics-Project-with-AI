using UnityEngine;

namespace Filo{

    [AddComponentMenu("Filo Cables/Cable Solver")]
    public class CableSolver : MonoBehaviour {
    
        public int iterations = 4;
        public float bias = 0.2f;
    
        public Cable[] cables;
       
        void FixedUpdate () { 

            for (int i = 0; i < cables.Length; ++i){
                if (cables[i] != null && cables[i].isActiveAndEnabled)
                    cables[i].UpdateCable(Time.fixedDeltaTime);
            }
    
            for (int j = 0; j < iterations; ++j){
                for (int i = 0; i < cables.Length; ++i){
                    if (cables[i] != null && cables[i].isActiveAndEnabled)
                        cables[i].Solve(Time.fixedDeltaTime, bias);
                }
            }

        }
       

    }
}
