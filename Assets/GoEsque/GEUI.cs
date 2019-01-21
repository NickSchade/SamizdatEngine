using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SamizdatEngine.GE.Basic;

namespace SamizdatEngine.GE
{
    public interface GEUI
    {
        void HandleUI();
        GoEsque game { get; set; }
    }
    

}
