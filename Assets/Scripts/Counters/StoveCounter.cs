using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;


    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;
    private float fryingTimer, burningTimer;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (!HasKitchenObject()) return;

        switch (state)
        {
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                });

                if (fryingTimer > fryingRecipeSO.fryingTimerMax)
                {
                    GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

                    burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
                    state = State.Fried;
                    burningTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        state = state
                    });
                }

                break;
            case State.Fried:
                burningTimer += Time.deltaTime;
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
                });

                if (burningTimer > burningRecipeSO.burningTimerMax)
                {
                    GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                    Debug.Log("object burned!");
                    state = State.Burned;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        state = state
                    });

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = 0f
                    });
                }

                break;
            case State.Burned:
                break;
        }

        Debug.Log(state);
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    state = State.Frying;
                    fryingTimer = 0f;
                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                    {
                        state = state
                    });

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax
                    });
                }
            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();

                        state = State.Idle;
                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                        {
                            state = state
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = 0f
                        });
                    }
                }
            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
                state = State.Idle;
                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
                {
                    state = state
                });
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO input)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(input);
        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO input)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(input);

        if (fryingRecipeSO != null)
        {
            return fryingRecipeSO.output;
        }

        return null;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO input)
    {
        foreach (var fryingRecipeSO in fryingRecipeSOArray)
        {
            if (fryingRecipeSO.input == input)
            {
                return fryingRecipeSO;
            }
        }

        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO input)
    {
        foreach (var burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == input)
            {
                return burningRecipeSO;
            }
        }

        return null;
    }
}