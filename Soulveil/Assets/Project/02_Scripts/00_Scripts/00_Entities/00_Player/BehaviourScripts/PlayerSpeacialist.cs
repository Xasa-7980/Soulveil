using System;
using UnityEngine;

public class PlayerSpeacialist : MonoBehaviour
{
    [SerializeField]
    private SpecialistHandler specialistHandler;

    public SpecialistHandler CurrentSpecialist => specialistHandler;

    public int lightAttackLength => specialistHandler != null ? specialistHandler.lightAttackLength : 0;
    public int heavyAttackLength => specialistHandler != null ? specialistHandler.heavyAttackLength : 0;


    public event EventHandler<OnChangeSpecialistEventArgs>
        OnChangeSpecialist;


    public class OnChangeSpecialistEventArgs : EventArgs
    {
        public SpecialistHandler specialistHandler;
    }


    private void Start ( )
    {
        // Aplicamos también el especialista inicial.
        NotifySpecialistChanged();
    }


    public void SetNewSpecialist (
        SpecialistHandler newSpecialist )
    {
        if (newSpecialist == specialistHandler)
            return;

        specialistHandler = newSpecialist;

        NotifySpecialistChanged();
    }


    private void NotifySpecialistChanged ( )
    {
        OnChangeSpecialist?.Invoke(
            this,
            new OnChangeSpecialistEventArgs
            {
                specialistHandler = specialistHandler
            }
        );
    }
}