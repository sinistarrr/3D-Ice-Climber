using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlyingBlockBehaviour : MonoBehaviour
{
    private float xBlockTranslationSpeed, yBlockTranslationSpeed, zBlockTranslationSpeed;
    private float xBlockRotationSpeed, yBlockRotationSpeed, zBlockRotationSpeed;
    private float blockTranslationRange = 5.0f;
    private float blockRotationRange = 200.0f;
    private GameObject childOfBlock;
    private Material childOfBlockMaterial;
    private bool isDisappearing = false;
    private float elapsedTime = 0;
    private float startValue;
    private float duration = 0.5f; // lerping duration
    private float selfDestructTime = 2.0f; // time before it self destructs

    // Start is called before the first frame update
    void Start()
    {
        childOfBlock = transform.GetChild(0).gameObject;
        // We randomly choose translation values for the block at the beginning between a defined range.
        xBlockTranslationSpeed = Random.Range(-blockTranslationRange, blockTranslationRange);
        yBlockTranslationSpeed = Random.Range(-blockTranslationRange, blockTranslationRange);
        zBlockTranslationSpeed = Random.Range(-blockTranslationRange, -1.0f);

        // We randomly choose rotation values for the block at the beginning between a defined range.
        xBlockRotationSpeed = Random.Range(-blockRotationRange, blockRotationRange);
        yBlockRotationSpeed = Random.Range(-blockRotationRange, blockRotationRange);
        zBlockRotationSpeed = Random.Range(-blockRotationRange, blockRotationRange);

        StartCoroutine(SelfDestruct(selfDestructTime));

    }

    // Update is called once per frame
    void Update()
    {
        // Manipulation of the block's translation
        transform.Translate(new Vector3(xBlockTranslationSpeed, yBlockTranslationSpeed, zBlockTranslationSpeed) * Time.deltaTime);
        // Manipulation of the block's rotation
        childOfBlock.transform.Rotate(new Vector3(xBlockRotationSpeed, yBlockRotationSpeed, zBlockRotationSpeed) * Time.deltaTime);

        if (isDisappearing)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, 0, elapsedTime / duration);
            childOfBlockMaterial.color = new Color(childOfBlockMaterial.color.r, childOfBlockMaterial.color.g, childOfBlockMaterial.color.b, newAlpha);
        }
    }

    private IEnumerator SelfDestruct(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);
        childOfBlockMaterial = childOfBlock.GetComponent<MeshRenderer>().material;
        startValue = childOfBlock.GetComponent<MeshRenderer>().material.color.a;
        ChangeMaterialBlendingModeToTransparent(childOfBlockMaterial);
        isDisappearing = true;
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);


    }

    private void ChangeMaterialBlendingModeToTransparent(Material currentMat){
        currentMat.SetFloat("_Mode", 3);
        currentMat.SetOverrideTag("RenderType", "Transparent");
        currentMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        currentMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        currentMat.SetFloat("_ZWrite", 0.0f);
        currentMat.DisableKeyword("_ALPHATEST_ON");
        currentMat.DisableKeyword("_ALPHABLEND_ON");
        currentMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        currentMat.renderQueue = 3000;
    }
}
