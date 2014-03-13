using UnityEngine;
using System.Collections;

public class MakePersistentScript : MonoBehaviour
{

    private MakePersistentScript thisScript;

    void Awake()
    {
        //Only allow ONE instance of this script, ever(!)
        if (thisScript != null && thisScript != this)
        {
            Destroy(this.gameObject);
            return;
        }
        thisScript = this;

        DontDestroyOnLoad(this);
    }


}
