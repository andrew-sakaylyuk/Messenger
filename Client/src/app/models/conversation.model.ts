export class Conversation {

  constructor(
     public Text="",
     public FirstName="",
     public LastName="",
     public DateTime="",
     public OtherUserId=0,
     public NewMessagesCount=0,
     public AvatarUrl="",
     public Online=false 
  ){}

  toString() {
    return JSON.stringify(this);
  }

}
