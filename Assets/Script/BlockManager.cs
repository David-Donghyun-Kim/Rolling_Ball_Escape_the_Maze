using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public Vector3 blockChunkScale;
    float timer;
    float waitingTime;

    // Start is called before the first frame update
    void Start()
    {
        blockChunkScale = gameObject.GetComponent<Transform>().localScale;
        timer = 0.0f;
        waitingTime = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > waitingTime)
        {
            blockChunkScale.x -= 0.02f;
            blockChunkScale.y -= 0.02f;
            blockChunkScale.z -= 0.02f;
            gameObject.transform.localScale = blockChunkScale;
            timer = 0;
        }
    }
}
