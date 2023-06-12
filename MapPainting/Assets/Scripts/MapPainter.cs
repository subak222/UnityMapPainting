using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPainter : MonoBehaviour
{
    public Texture2D mapTexture; // 맵을 표현하는 텍스처
    public Color paintColor = Color.red; // 페인트할 색상
    public float paintRadius = 10f; // 페인트 반경

    private Renderer mapRenderer; // 맵을 표시하는 렌더러

    private void Start()
    {
        // 초기화된 텍스처 생성
        mapTexture = new Texture2D(512, 512);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;
        ClearMap();

        // 맵을 표시하는 렌더러 가져오기
        mapRenderer = GetComponent<Renderer>();

        // 텍스처를 맵 렌더러의 메인 텍스처로 설정
        mapRenderer.material.mainTexture = mapTexture;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 위치를 텍스처 좌표로 변환
        Vector2 textureCoord = GetTextureCoordFromWorldPosition(collision.contacts[0].point);

        // 텍스처 좌표 범위 검사
        if (IsValidTextureCoord(textureCoord))
        {
            // 텍스처를 색칠할 중심 좌표
            int centerX = (int)textureCoord.x;
            int centerY = (int)textureCoord.y;

            // 텍스처 반경 내의 모든 픽셀에 대해 페인트 작업 수행
            for (int x = centerX - (int)paintRadius; x <= centerX + (int)paintRadius; x++)
            {
                for (int y = centerY - (int)paintRadius; y <= centerY + (int)paintRadius; y++)
                {
                    // 텍스처 좌표가 유효한 범위 내에 있는지 확인
                    if (IsValidTextureCoord(new Vector2(x, y)))
                    {
                        // 중심 좌표로부터의 거리 계산
                        float distance = Vector2.Distance(new Vector2(x, y), textureCoord);

                        // 페인트 작업 수행
                        if (distance <= paintRadius)
                        {
                            float alpha = 1f - (distance / paintRadius);
                            Color pixelColor = mapTexture.GetPixel(x, y);
                            Color newColor = Color.Lerp(pixelColor, paintColor, alpha);
                            mapTexture.SetPixel(x, y, newColor);
                        }
                    }
                }
            }

            // 변경된 픽셀 적용
            mapTexture.Apply();

            // 충돌한 오브젝트의 색상 변경
            collision.gameObject.GetComponent<Renderer>().material.color = paintColor;
        }
    }
    private Vector2 GetTextureCoordFromWorldPosition(Vector3 worldPosition)
    {
        // 맵의 로컬 좌표로 변환
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

        // 로컬 좌표를 텍스처 좌표로 변환
        float u = (localPosition.x + 0.5f) * mapTexture.width;
        float v = (localPosition.z + 0.5f) * mapTexture.height;

        return new Vector2(u, v);
    }

    private bool IsValidTextureCoord(Vector2 textureCoord)
    {
        // 텍스처 좌표가 유효한 범위 내에 있는지 확인
        return textureCoord.x >= 0 && textureCoord.x < mapTexture.width &&
            textureCoord.y >= 0 && textureCoord.y < mapTexture.height;
    }
    private void ClearMap()
    {
        Color[] pixels = mapTexture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        mapTexture.SetPixels(pixels);
        mapTexture.Apply();
    }
}