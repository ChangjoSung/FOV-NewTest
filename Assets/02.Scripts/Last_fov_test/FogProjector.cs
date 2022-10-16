using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogProjector : MonoBehaviour
{
    // 렌더 텍스처
    public RenderTexture fogTexture;

    // 잔상 표현을 위한 두 렌더 텍스처
    RenderTexture projecTexture;
    RenderTexture oldTexture;

    // 전장의 안개와 시야를 블러 셰이더로 섞어 자연스러운 모습을 연출해야 한다.
    // 지금은 아직 언급하지 않았다.
    public Shader blurShader;

    [Range(1, 4)]
    public int upsample = 2;

    Material blurMaterial;
    public float blur=1;

    Projector projector;

    public float blendSpeed = 1;
    float blend;
    int blendNameId;

    void OnEnable()
    {
        projector = GetComponent<Projector>();

        // 블러 셰이더를 적용한 머터리얼 생성
        // 블러 셰어더
        // field of view로 확보된 시야와 안개의 경계를 잘 섞어 부드럽게 보이도록 하는 역할을 담당한다.
        // (field of view를 볼 때 시야 영역이 원이 아닌 다각형이므로 블러 셰이더를 적용하지 않으면 울퉁불퉁하게 보인다.)
        blurMaterial = new Material(blurShader);
        blurMaterial.SetVector("_Parameter", new Vector4(blur, -blur, 0, 0));

        // 새로운 렌더 텍스처 객체 생성
        // 필터 모드는 Bilinear로 똑같이 맞춰준다.
        projecTexture = new RenderTexture(
                            fogTexture.width * upsample,
                            fogTexture.height * upsample,
                            0,
                            fogTexture.format) {filterMode = FilterMode.Bilinear};

        oldTexture = new RenderTexture(
                        fogTexture.width * upsample,
                        fogTexture.height * upsample,
                        0,
                        fogTexture.format) {filterMode = FilterMode.Bilinear};
        
        // 두 텍스처를 각각 _FogTex, _OldFogTex에 할당하도록 한다.
        projector.material.SetTexture("_FogTex", projecTexture);
        projector.material.SetTexture("_OldFogTex", oldTexture);

        // 셰이더의 _Blend 변수를 연결하고 현재 안개 텍스처를 projectTexture로 옮긴다.
        blendNameId = Shader.PropertyToID("_Blend");
        blend = 1;
        projector.material.SetFloat(blendNameId, blend);
        Graphics.Blit(fogTexture, projecTexture);
        UpdateFog();
    }

    // 두 텍스처를 섞어서 잔상을 구현하는 함수
    public void UpdateFog()
    {
        Graphics.Blit(projecTexture, oldTexture);
        Graphics.Blit(fogTexture, projecTexture);

        RenderTexture temp = RenderTexture.GetTemporary(
            projecTexture.width,
            projecTexture.height,
            0,
            projecTexture.format);

        temp.filterMode = FilterMode.Bilinear;

        Graphics.Blit(projecTexture, temp, blurMaterial, 1);
        Graphics.Blit(temp, projecTexture, blurMaterial, 2);

        StartCoroutine(Blend());

        RenderTexture.ReleaseTemporary(temp);
    }

    // 코루틴으로 텍스처를 섞는 과정을 자연스럽게 구현한다.
    IEnumerator Blend()
    {
        blend = 0;
        projector.material.SetFloat(blendNameId, blend);
        while (blend < 1)
        {
            blend = Mathf.MoveTowards(blend, 1, blendSpeed * Time.deltaTime);
            projector.material.SetFloat(blendNameId, blend);
            yield return null;
        }
    }
}