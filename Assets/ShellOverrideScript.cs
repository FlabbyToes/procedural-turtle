using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShellOverrideScript : MonoBehaviour
{
    [SerializeField] private List<Transform> ShellCopies;
    [SerializeField] private List<Transform> ShellCopiesBase;
    // Start is called before the first frame update
    [Range(-10f, 10f)][SerializeField] private float min_position;
    [Range(-10f, 10f)][SerializeField] private float max_position;
    [SerializeField] private AnimationCurve shellAnimationCurve;
    [SerializeField] float fluctuateTime;
    private List<bool> isMoving;
    private List<bool> fluctuatePosition;
    private enum STATE {
        STATE_FLUCTUATING
    }
    void Start()
    {
        isMoving = new List<bool>();
        fluctuatePosition = new List<bool>();
        for (int i = 0; i < ShellCopies.Count; ++i)
        {
            isMoving.Add(false);
            fluctuatePosition.Add(false);
            if (i%2 == 0)
            {
                fluctuatePosition[i] = true;
            }
        }
    }
    void fluctuateShellHeight()
    {
        for (int i = 0; i < ShellCopies.Count; ++i)
        {
            float position = min_position;
            if (fluctuatePosition[i])
            {
                position = max_position;
            }
            if (!isMoving[i])
            {
                StartCoroutine(movingIK(i, position, (ShellCopies[i].position - ShellCopiesBase[i].position).magnitude, fluctuateTime));
            }
        }
    }
    IEnumerator movingIK(int i, float position, float startPos, float updateTime)
    {
        isMoving[i] = true;
        float elapsedTime = 0f;

        while (elapsedTime < updateTime)
        {
            ShellCopies[i].position = Vector3.Lerp(ShellCopiesBase[i].position - ShellCopies[i].up * startPos,
                ShellCopiesBase[i].position + ShellCopies[i].up * position, shellAnimationCurve.Evaluate(elapsedTime / updateTime));
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        ShellCopies[i].position = ShellCopiesBase[i].position + ShellCopies[i].up * position;
        isMoving[i] = false;
        fluctuatePosition[i] = !fluctuatePosition[i];
        yield return new WaitForFixedUpdate();
    }
        // Update is called once per frame
    void FixedUpdate()
    {
        fluctuateShellHeight();
    }
}
