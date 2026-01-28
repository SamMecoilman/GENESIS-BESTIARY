using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    /// <summary>
    /// Input System のデバッグ用スクリプト
    /// OnLook/OnMoveメッセージが呼ばれているか確認
    /// </summary>
    public class InputDebugger : MonoBehaviour
    {
        [Header("Debug Info")]
        public Vector2 lastLookInput;
        public Vector2 lastMoveInput;
        public int lookCallCount;
        public int moveCallCount;
        public bool isMousePresent;
        public string mousePosition;
        
        private void Update()
        {
            // マウスの存在確認
            isMousePresent = Mouse.current != null;
            if (isMousePresent)
            {
                var mouse = Mouse.current;
                mousePosition = $"Pos: {mouse.position.ReadValue()}, Delta: {mouse.delta.ReadValue()}";
            }
            else
            {
                mousePosition = "Mouse not found!";
            }
        }
        
        // PlayerInput SendMessages から呼ばれる
        public void OnLook(InputValue value)
        {
            lookCallCount++;
            lastLookInput = value.Get<Vector2>();
            Debug.Log($"[InputDebugger] OnLook called! Value: {lastLookInput}, Count: {lookCallCount}");
        }
        
        // PlayerInput SendMessages から呼ばれる
        public void OnMove(InputValue value)
        {
            moveCallCount++;
            lastMoveInput = value.Get<Vector2>();
            Debug.Log($"[InputDebugger] OnMove called! Value: {lastMoveInput}, Count: {moveCallCount}");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 200));
            GUILayout.Label($"=== Input Debugger ===");
            GUILayout.Label($"Mouse Present: {isMousePresent}");
            GUILayout.Label($"{mousePosition}");
            GUILayout.Label($"Look Input: {lastLookInput} (calls: {lookCallCount})");
            GUILayout.Label($"Move Input: {lastMoveInput} (calls: {moveCallCount})");
            GUILayout.EndArea();
        }
    }
}
