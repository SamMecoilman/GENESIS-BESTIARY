using GenesisBestiary.Combat;
using GenesisBestiary.Utilities;
using UnityEngine;

namespace GenesisBestiary.Player
{
    public enum PlayerState
    {
        Idle,
        Move,
        Attack,
        Dodge,
        Carve,
        Stagger,
        Dead
    }

    public sealed class PlayerStateMachine : StateMachine<PlayerState>
    {
        public PlayerStateMachine(PlayerController controller)
        {
            States[PlayerState.Idle] = new PlayerIdleState(this, controller);
            States[PlayerState.Move] = new PlayerMoveState(this, controller);
            States[PlayerState.Attack] = new PlayerAttackState(this, controller);
            States[PlayerState.Dodge] = new PlayerDodgeState(this, controller);
            States[PlayerState.Carve] = new PlayerCarveState(this, controller);
            States[PlayerState.Dead] = new PlayerDeadState(this, controller);
            ChangeState(PlayerState.Idle);
        }

        public void RequestStateChange(PlayerState newState)
        {
            ChangeState(newState);
        }

        #region Idle State
        private sealed class PlayerIdleState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;

            public PlayerIdleState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter() { }

            public void Tick()
            {
                // Check for attack input
                if (controller.AttackInput)
                {
                    if (controller.Combat != null && controller.Combat.TryStartAttack())
                    {
                        stateMachine.ChangeState(PlayerState.Attack);
                        return;
                    }
                }
                
                // Check for dodge input
                if (controller.DodgeInput && controller.CanDodge)
                {
                    stateMachine.ChangeState(PlayerState.Dodge);
                    return;
                }
                
                // Check for movement
                if (controller.HasMoveInput)
                {
                    stateMachine.ChangeState(PlayerState.Move);
                }
            }

            public void Exit() { }
        }
        #endregion

        #region Move State
        private sealed class PlayerMoveState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;

            public PlayerMoveState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter() { }

            public void Tick()
            {
                // Check for attack input
                if (controller.AttackInput)
                {
                    if (controller.Combat != null && controller.Combat.TryStartAttack())
                    {
                        stateMachine.ChangeState(PlayerState.Attack);
                        return;
                    }
                }
                
                // Check for dodge input
                if (controller.DodgeInput && controller.CanDodge)
                {
                    stateMachine.ChangeState(PlayerState.Dodge);
                    return;
                }
                
                // Check for stop
                if (!controller.HasMoveInput)
                {
                    stateMachine.ChangeState(PlayerState.Idle);
                    return;
                }

                Vector3 moveDirection = controller.GetCameraRelativeMove();
                controller.Move(moveDirection);
                controller.RotateTowards(moveDirection);
            }

            public void Exit() { }
        }
        #endregion

        #region Attack State
        private sealed class PlayerAttackState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;

            public PlayerAttackState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                if (controller.Combat != null)
                {
                    controller.Combat.OnAttackEnd += OnAttackEnd;
                }
            }

            public void Tick()
            {
                if (controller.Combat != null)
                {
                    controller.Combat.UpdateAttack();
                    
                    // Apply forward movement during attack if specified
                    var currentAttack = controller.Combat.CurrentAttack;
                    if (currentAttack != null && currentAttack.forwardMovement > 0f)
                    {
                        Vector3 forward = controller.transform.forward * currentAttack.forwardMovement * Time.deltaTime;
                        controller.MoveRaw(forward);
                    }
                }
            }

            public void Exit()
            {
                if (controller.Combat != null)
                {
                    controller.Combat.OnAttackEnd -= OnAttackEnd;
                }
            }
            
            private void OnAttackEnd()
            {
                stateMachine.ChangeState(PlayerState.Idle);
            }
        }
        #endregion

        #region Dodge State
        private sealed class PlayerDodgeState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;
            private float dodgeTimer;
            private Vector3 dodgeDirection;

            public PlayerDodgeState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                dodgeTimer = 0f;
                controller.ConsumeStamina(controller.Data.dodgeStaminaCost);
                controller.SetInvincible(true);
                
                // Get dodge direction (input direction or forward if no input)
                dodgeDirection = controller.HasMoveInput 
                    ? controller.GetCameraRelativeMove() 
                    : controller.transform.forward;
                    
                dodgeDirection.Normalize();
            }

            public void Tick()
            {
                dodgeTimer += Time.deltaTime;
                
                // End invincibility after iframes
                if (dodgeTimer >= controller.Data.dodgeIFrameDuration)
                {
                    controller.SetInvincible(false);
                }
                
                // Move during dodge
                if (dodgeTimer < controller.Data.dodgeDuration)
                {
                    float speed = controller.Data.dodgeDistance / controller.Data.dodgeDuration;
                    controller.MoveRaw(dodgeDirection * speed * Time.deltaTime);
                }
                else
                {
                    // Dodge complete
                    stateMachine.ChangeState(PlayerState.Idle);
                }
            }

            public void Exit()
            {
                controller.SetInvincible(false);
            }
        }
        #endregion

        #region Carve State
        private sealed class PlayerCarveState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;
            private CarvingPoint currentCarvingPoint;

            public PlayerCarveState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                currentCarvingPoint = controller.CurrentCarvingPoint;
                if (currentCarvingPoint != null)
                {
                    currentCarvingPoint.StartCarving();
                    currentCarvingPoint.OnCarveComplete += OnCarveComplete;
                }
            }

            public void Tick()
            {
                if (currentCarvingPoint == null)
                {
                    stateMachine.ChangeState(PlayerState.Idle);
                    return;
                }

                currentCarvingPoint.UpdateCarving(Time.deltaTime);

                // Cancel if player inputs movement or dodge
                if (controller.HasMoveInput || controller.DodgeInput)
                {
                    currentCarvingPoint.CancelCarving();
                    stateMachine.ChangeState(PlayerState.Idle);
                }
            }

            public void Exit()
            {
                if (currentCarvingPoint != null)
                {
                    currentCarvingPoint.OnCarveComplete -= OnCarveComplete;
                }
                currentCarvingPoint = null;
            }

            private void OnCarveComplete()
            {
                // Check if more carves available
                if (currentCarvingPoint != null && currentCarvingPoint.CanCarve)
                {
                    // Stay in carve state for next carve if player holds button
                    if (controller.CarveInput)
                    {
                        currentCarvingPoint.StartCarving();
                    }
                    else
                    {
                        stateMachine.ChangeState(PlayerState.Idle);
                    }
                }
                else
                {
                    stateMachine.ChangeState(PlayerState.Idle);
                }
            }
        }
        #endregion

        #region Dead State
        private sealed class PlayerDeadState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;

            public PlayerDeadState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                // Disable player controls
                Debug.Log("Player entered Dead state");
            }

            public void Tick()
            {
                // Wait for respawn or game over
            }

            public void Exit()
            {
            }
        }
        #endregion
    }
}
