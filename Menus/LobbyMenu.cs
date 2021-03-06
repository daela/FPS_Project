using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class LobbyMenu : Container, IMenu {
  Godot.TextEdit messageBox;
  Godot.TextEdit composeBox;
  Godot.Button sendButton;
  Godot.Button mainMenuButton;
  Godot.TextEdit playersBox;
  Godot.Button readyButton;

  // Server stuff
  IMenu arenaConfig;
  

  private bool countDownActive = false;
  private float timer = 0f;
  private int countDown = 10;
  private bool isReady = false;


  private string myName;
  private List<string> messages;

  public override void _Ready() {
    messages = new List<string>();
  }

  public override void _Process(float delta){
    if(countDownActive){
      CountDown(delta);
    }
    
  }

  void CountDown(float delta){
    timer += delta;
    if(timer > 1f){
      countDown--;
      timer = 0;
      BuildPlayers();
    }
    if(countDown < 1){
      countDownActive = false;
      StartGame();
    }
  }

  public void Init(float minX, float minY, float maxX, float maxY){
    NetworkSession netSes = Session.session.netSes;
    if(netSes.isServer){
      InitServerControls();
    }
    else{
      InitControls();
    } 
    InitNetwork();
    if(netSes.isServer){
      ScaleServerControls();
    }
    else{
      ScaleControls();
    }
    
  }
  
  public void Resize(float minX, float minY, float maxX, float maxY){

  }

  public bool IsSubMenu(){
    return false;
  }

  public void Clear(){
    this.QueueFree();
  }

  void InitControls(){
    messageBox = (Godot.TextEdit)Menu.TextBox();
    messageBox.Readonly = true;
    AddChild(messageBox);
    
    composeBox = (Godot.TextEdit)Menu.TextBox("", false);
    AddChild(composeBox);
    
    sendButton = (Godot.Button)Menu.Button("Send", Send);
    AddChild(sendButton);
    
    mainMenuButton = (Godot.Button)Menu.Button("Main Menu", ReturnToMainMenu);
    AddChild(mainMenuButton);

    readyButton = (Godot.Button)Menu.Button("Ready", ToggleReady);
    AddChild(readyButton);
    
    playersBox = (Godot.TextEdit)Menu.TextBox("");
    playersBox.Readonly = true;
    AddChild(playersBox);
  }

  void InitServerControls(){
    messageBox = (Godot.TextEdit)Menu.TextBox();
    messageBox.Readonly = true;
    AddChild(messageBox);
    
    
    mainMenuButton = (Godot.Button)Menu.Button("Main Menu", ReturnToMainMenu);
    AddChild(mainMenuButton);
    
    playersBox = (Godot.TextEdit)Menu.TextBox("");
    playersBox.Readonly = true;
    AddChild(playersBox);

    Node arenaConfigNode = Menu.SubMenuFactory(Menu.SubMenus.ArenaConfig);
    AddChild(arenaConfigNode);
    arenaConfig = arenaConfigNode as IMenu;
  }

  void InitNetwork(){
    if(Session.session.netSes == null){
      GD.Print("No network session found in lobby menu");
      return;
    }
    NetworkSession netSes = Session.session.netSes;


    if(Session.session.netSes.Initialized()){
      if(netSes.isServer){
        netSes.UpdateServer(obj: this, playerJoin: nameof(PlayerJoined), playerLeave: nameof(PlayerQuit));
        ReceiveMessage("Server still using random seed: " + netSes.randomSeed);
        foreach(KeyValuePair<int, PlayerData> entry in netSes.playerData){
          string json = JsonConvert.SerializeObject(entry.Value, Formatting.Indented);
          AddPlayer(json);
          Rpc(nameof(AddPlayer), json);
        }

      }
      else{
        netSes.UpdateClient(obj: this, success: nameof(ConnectionSucceeded), fail: nameof(ConnectionFailed), peerJoin : nameof(PeerConnected));
      }
      BuildPlayers();
      return;
    }
    
    
<<<<<<< HEAD
    public void ReturnToMainMenu(){
      Session.session.ChangeMenu(Menu.Menus.Main);
=======
    myName = "Server";

    if(netSes.isServer){
      netSes.InitServer(obj: this, playerJoin: nameof(PlayerJoined), playerLeave: nameof(PlayerQuit), port : netSes.initPort);
      ReceiveMessage("Server initialized with random seed: " + netSes.randomSeed);
>>>>>>> develop
    }
    else{
      netSes.InitClient(address: netSes.initAddress, obj: this, success: nameof(ConnectionSucceeded), fail: nameof(ConnectionFailed), peerJoin : nameof(PeerConnected),  port: netSes.initPort);
    }
    BuildPlayers();
  }
  
  void ScaleControls(){
    Rect2 screen = this.GetViewportRect();
    float width = screen.Size.x;
    float height = screen.Size.y;
    float wu = width/10; // relative height and width units
    float hu = height/10;
    
    Menu.ScaleControl(mainMenuButton, 2 * wu, hu, 0, height - hu);
    Menu.ScaleControl(composeBox, 6 * wu, 2 * hu, 3 * wu, 8 * hu);
    Menu.ScaleControl(messageBox, 6 * wu, 8 * hu, 3 * wu, 0);
    Menu.ScaleControl(sendButton, wu, 2 * hu, 9 * wu, 8 * hu);
    Menu.ScaleControl(playersBox, 2 * wu, 8 * hu, 0, 0);
    Menu.ScaleControl(readyButton, wu, hu, 2 * wu, 0);
  }

  void ScaleServerControls(){
    Rect2 screen = this.GetViewportRect();
    float width = screen.Size.x;
    float height = screen.Size.y;
    float wu = width/10; // relative height and width units
    float hu = height/10;
    
    Menu.ScaleControl(mainMenuButton, 2 * wu, hu, 0, height - hu);
    Menu.ScaleControl(messageBox, 6 * wu, hu, 3 * wu, 0);
    Menu.ScaleControl(playersBox, 2 * wu, 8 * hu, 0, 0);

    arenaConfig.Init(2 * wu, hu, width, height);
  }
  
  public void ReturnToMainMenu(){
    Session.ChangeMenu(Menu.Menus.Main);
  }
  
  public void Send(){
    if(composeBox != null && composeBox.GetText() != ""){
      
      string message = composeBox.GetText(); 
      ReceiveNamedMessage(message, myName);
      Rpc(nameof(ReceiveNamedMessage), message, myName);
      composeBox.SetText("");
    }
  }
  
  public void PlayerJoined(int id){
  }

  [Remote]
  public void InitRandomSeed(int randomSeed){
    Session.session.netSes.InitRandom(randomSeed);
  }

  public void PlayerQuit(int id){
    //ReceiveMessage("Player " + id + " quit.");
    RemovePlayer(id);
    Rpc(nameof(RemovePlayer), id);
  }

  /* Called before peers connect. */
  public void ConnectionSucceeded(){
    NetworkSession netSes = Session.session.netSes;
    myName = netSes.initName;
    int myId = netSes.selfPeerId;

    if(myName == "Name"){
      myName = "Player #" + myId.ToString();
    }

    PlayerData dat = new PlayerData(myName, myId);
    string json = JsonConvert.SerializeObject(dat, Formatting.Indented);
    AddPlayer(json);
    Rpc(nameof(AddPlayer), json);

    string message = myName + " joined!"; 
    ReceiveMessage(message);
    Rpc(nameof(ReceiveMessage), message);
  }

  public void PeerConnected(int id){
    if(id == 1){
      return; // Don't worry about the server.
    }
    int myId = Session.session.netSes.selfPeerId;
    PlayerData myDat = Session.session.netSes.playerData[myId];
    string myJson = JsonConvert.SerializeObject(myDat, Formatting.Indented);
    RpcId(id, nameof(AddPlayer), myJson);
    StopCountDown();
  }

  [Remote]
  public void ResetReady(){
    NetworkSession netSes = Session.session.netSes;

    foreach(KeyValuePair<int, PlayerData> entry in netSes.playerData){
      entry.Value.ready = false;
    }
  }


  public void ConnectionFailed(){
    ReceiveMessage("Connection failed. :(");
  }

  [Remote]
  public void ReceiveMessage(string message){
    if(messageBox == null){
      return;
    }
    if(messages.Count > 50){ messages.Remove(messages.First()); }
    messages.Add(message);
    string str = "";
    for(int i = 0; i < messages.Count; i++){
      str += messages[i] + "\n";
    }
    messageBox.SetText(str);
  }
  
  [Remote]
  public void ReceiveNamedMessage(string message, string name){
    string fullMessage = name + ": " + message;
    ReceiveMessage(fullMessage);  
  }
  
  [Remote]
  public void AddPlayer(string json){
    PlayerData dat = JsonConvert.DeserializeObject<PlayerData>(json);
    
    if(dat == null){
      GD.Print("AddPlayer: PlayerData null");
      return;
    }

    NetworkSession netSes = Session.session.netSes;
    if(netSes == null){ 
      GD.Print("AddPlayer: No network session detected");
      return; 
    }
    if(dat.id == netSes.selfPeerId){
      myName = dat.name;
    }

    if(!netSes.playerData.ContainsKey(dat.id)){
      netSes.playerData.Add(dat.id, dat);
    }
    
    
    BuildPlayers();
  }

  [Remote]
  public void RemovePlayer(int id){
    Session.session.netSes.playerData.Remove(id);
    BuildPlayers();
  }

  void ToggleReady(){
    isReady = !isReady;
    int myId = Session.session.netSes.selfPeerId;
    
    TogglePlayerReady(myId);
    Rpc(nameof(TogglePlayerReady), myId);
    
    if(isReady){  
      readyButton.SetText("Waiting");
    }
    else{
      readyButton.SetText("Ready");
    }

<<<<<<< HEAD
    public void StartGame(){
      ResetReady();
      Session.session.MultiPlayerGame();
    }
=======
  }
>>>>>>> develop

  [Remote]
  public void TogglePlayerReady(int playerId){
    PlayerData dat = Session.session.netSes.playerData[playerId];
    
    if(dat == null){
      GD.Print("Player " + playerId + " doesn't exist.");
      return;
    }

    dat.ready = !dat.ready;
    string message = dat.name;
    message += dat.ready ? " is ready." : " is not ready.";

    if(AllPlayersReady()){
      StartCountDown();

    }
    else{
      StopCountDown();
    }

    ReceiveMessage(message);
  }

  public bool AllPlayersReady(){
    NetworkSession netSes = Session.session.netSes;
    foreach(KeyValuePair<int, PlayerData> entry in netSes.playerData){
      if(entry.Value.ready == false){
        return false;
      }
    }
    return true;
  }

  public void StartGame(){
    ResetReady();
    Session.MultiplayerArena();
  }

  void StopCountDown(){
    countDownActive = false;
    BuildPlayers();
  }

  [Remote]
  void StartCountDown(){
    timer = 0f;
    countDown = 10;
    countDownActive = true;
    BuildPlayers();
  }

  [Remote]
  void PrintPlayer(string dat){
    PlayerData playerDat = JsonConvert.DeserializeObject<PlayerData>(dat);
  }


  void BuildPlayers(){
    NetworkSession netSes = Session.session.netSes;
    string names = "Players(" + netSes.playerData.Count + ")";
    
    if(countDownActive){
      names += " Starting in " + countDown;
    }

    names += "\n";
    
    foreach(KeyValuePair<int, PlayerData> entry in netSes.playerData){
      names += entry.Value.name + "\n";
    }
    playersBox.SetText(names);
  }
    
}
