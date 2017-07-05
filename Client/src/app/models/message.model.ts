export class Message {

  constructor(
     public Id : Number,
     public DateTime: string,
     public Text: string,
     public FirstName: string,
     public SenderId: Number,
     public New: boolean
  ){}

  toString() {
    return JSON.stringify(this);
  }

}