using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldEntityAction 
{
    public DecentralandEntity entity;
    public object oldValue, newValue;


    public BuilderInWorldEntityAction(DecentralandEntity entity)
    {
        this.entity = entity;
    }
    public BuilderInWorldEntityAction(DecentralandEntity entity,object oldValue,object newValue)
    {
        this.entity = entity;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }
}