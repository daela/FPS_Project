using Godot;
using System;
/*
    Unless local multiplayer is added in the future, there should only be
    one instance of this class at a time.
*/

public class Eyes : Camera {
  private Vector2 mousePosition;
  
  public override void _Ready(){
    Input.SetMouseMode(Input.MouseMode.Captured);
  }
  
  public override void _Process(float delta){
    mousePosition = this.GetViewport().GetMousePosition();
  }
  
  public Vector2 GetMousePosition(){
    return mousePosition;
  }
}
