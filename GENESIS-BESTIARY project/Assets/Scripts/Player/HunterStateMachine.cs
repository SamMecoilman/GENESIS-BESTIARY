using GenesisBestiary.Combat;
using GenesisBestiary.Utilities;
using UnityEngine;

namespace GenesisBestiary.Player
{
    public enum HunterState
    {
        Locomotion,  // 移動中（Starter Assetsに任せる）
        Attack,      // 攻撃中
        Dodge,       // 回避中
        Carve,       // 剥ぎ取り中
        Stagger,     // のけぞり
        Dead         // 死亡
    }

    public sealed class HunterStateMachine : StateMachine<HunterState>
    {
        private readonly HunterController hunter;
        
        public bool CanMove => CurrentKey == HunterState.Locomotion;
        
        public HunterStateMachine(HunterController hunter)
        {
            this.hunter = hunter;
            
            States[HunterState.Locomotion] = new LocomotionState(this, hunter);
            States[HunterState.Attack] = new AttackState(this, hunter);
            States[HunterState.Dodge] = new DodgeState(this, hunter);
            States[HunterState.Carve] = new CarveState(this, hunter);
            States[HunterState.Stagger] = new StaggerState(this, hunter);
            States[HunterState.Dead] = new DeadState(this, hunter);
            
            ChangeState(HunterState.Locomotion);
        }
        
        public void RequestStateChange(HunterState newState)
        {
            ChangeState(newState);
        }
        
        #region Locomotion State
        private sealed class LocomotionState : IState
        {
            private readonly HunterStateMachine sm;
            private readonly HunterController hunter;
            
            public LocomotionState(HunterStateMachine sm, HunterController hunter)
            {
                this.sm = sm;
                this.hunter = hunter;
            }
            
            public void Enter() { }
            
            public void Tick()
            {
                // 攻撃入力
                if (hunter.AttackInput)
                {
                    if (hunter.Combat != null && hunter.Combat.TryStartAttack())
                    {
                        sm.ChangeState(HunterState.Attack);
                        return;
                    }
                }
                
                // 回避入力
                if (hunter.DodgeInput && hunter.CanDodge)
                {
                    sm.ChangeState(HunterState.Dodge);
                    return;
                }
            }
            
            public void Exit() { }
        }
        #endregion
        
        #region Attack State
        private sealed class AttackState : IState
        {
            private readonly HunterStateMachine sm;
            private readonly HunterController hunter;
            
            public AttackState(HunterStateMachine sm, HunterController hunter)
            {
                this.sm = sm;
                this.hunter = hunter;
            }
            
            public void Enter()
            {
                if (hunter.Combat != null)
                {
                    hunter.Combat.OnAttackEnd += OnAttackEnd;
                    hunter.Combat.OnComboAdvance += OnComboAdvance;
                }
                
                // AnimatorにAttackトリガーを送る
                if (hunter.Animator != null)
                {
                    hunter.Animator.SetTrigger("Attack");
                }
            }
            
            public void Tick()
            {
                if (hunter.Combat != null)
                {
                    hunter.Combat.UpdateAttack();
                    
                    // 攻撃中の前進移動
                    float forwardMovement = hunter.Combat.CurrentForwardMovement;
                    if (forwardMovement > 0f)
                    {
                        Vector3 forward = hunter.transform.forward * forwardMovement * Time.deltaTime;
                        hunter.MoveRaw(forward);
                    }
                }
            }
            
            public void Exit()
            {
                if (hunter.Combat != null)
                {
                    hunter.Combat.OnAttackEnd -= OnAttackEnd;
                    hunter.Combat.OnComboAdvance -= OnComboAdvance;
                }
            }
            
            private void OnAttackEnd()
            {
                sm.ChangeState(HunterState.Locomotion);
            }
            
            private void OnComboAdvance()
            {
                // コンボが進んだらAnimatorにトリガーを送る
                if (hunter.Animator != null)
                {
                    hunter.Animator.SetTrigger("Attack");
                }
            }
        }
        #endregion
        
        #region Dodge State
        private sealed class DodgeState : IState
        {
            private readonly HunterStateMachine sm;
            private readonly HunterController hunter;
            
            public DodgeState(HunterStateMachine sm, HunterController hunter)
            {
                this.sm = sm;
                this.hunter = hunter;
            }
            
            public void Enter()
            {
                hunter.StartDodge();
                
                // アニメーション
                if (hunter.Animator != null)
                {
                    hunter.Animator.SetTrigger("Dodge");
                }
            }
            
            public void Tick()
            {
                hunter.UpdateDodge();
                
                if (hunter.IsDodgeComplete())
                {
                    sm.ChangeState(HunterState.Locomotion);
                }
            }
            
            public void Exit()
            {
                hunter.SetInvincible(false);
            }
        }
        #endregion
        
        #region Carve State
        private sealed class CarveState : IState
        {
            private readonly HunterStateMachine sm;
            private readonly HunterController hunter;
            private CarvingPoint carvingPoint;
            
            public CarveState(HunterStateMachine sm, HunterController hunter)
            {
                this.sm = sm;
                this.hunter = hunter;
            }
            
            public void Enter()
            {
                carvingPoint = hunter.CurrentCarvingPoint;
                if (carvingPoint != null)
                {
                    carvingPoint.StartCarving();
                    carvingPoint.OnCarveComplete += OnCarveComplete;
                }
                
                // アニメーション
                if (hunter.Animator != null)
                {
                    hunter.Animator.SetTrigger("Carve");
                }
            }
            
            public void Tick()
            {
                if (carvingPoint == null)
                {
                    sm.ChangeState(HunterState.Locomotion);
                    return;
                }
                
                carvingPoint.UpdateCarving(Time.deltaTime);
                
                // 移動入力でキャンセル
                if (hunter.StarterInputs.move.sqrMagnitude > 0.1f || hunter.DodgeInput)
                {
                    carvingPoint.CancelCarving();
                    sm.ChangeState(HunterState.Locomotion);
                }
            }
            
            public void Exit()
            {
                if (carvingPoint != null)
                {
                    carvingPoint.OnCarveComplete -= OnCarveComplete;
                }
                carvingPoint = null;
                hunter.ClearCarvingPoint();
            }
            
            private void OnCarveComplete()
            {
                if (carvingPoint != null && carvingPoint.CanCarve && hunter.CarveInputHeld)
                {
                    // 続けて剥ぎ取り
                    carvingPoint.StartCarving();
                }
                else
                {
                    sm.ChangeState(HunterState.Locomotion);
                }
            }
        }
        #endregion
        
        #region Stagger State
        private sealed class StaggerState : IState
        {
            private readonly HunterStateMachine sm;
            private readonly HunterController hunter;
            private float staggerTimer;
            private const float STAGGER_DURATION = 0.5f;
            
            public StaggerState(HunterStateMachine sm, HunterController hunter)
            {
                this.sm = sm;
                this.hunter = hunter;
            }
            
            public void Enter()
            {
                staggerTimer = 0f;
                
                if (hunter.Animator != null)
                {
                    hunter.Animator.SetTrigger("Hit");
                }
            }
            
            public void Tick()
            {
                staggerTimer += Time.deltaTime;
                
                if (staggerTimer >= STAGGER_DURATION)
                {
                    sm.ChangeState(HunterState.Locomotion);
                }
            }
            
            public void Exit() { }
        }
        #endregion
        
        #region Dead State
        private sealed class DeadState : IState
        {
            private readonly HunterStateMachine sm;
            private readonly HunterController hunter;
            
            public DeadState(HunterStateMachine sm, HunterController hunter)
            {
                this.sm = sm;
                this.hunter = hunter;
            }
            
            public void Enter()
            {
                if (hunter.Animator != null)
                {
                    hunter.Animator.SetTrigger("Die");
                }
                
                Debug.Log("Hunter died!");
            }
            
            public void Tick()
            {
                // リスポーン待ち
            }
            
            public void Exit() { }
        }
        #endregion
    }
}
