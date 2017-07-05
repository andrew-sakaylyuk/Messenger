export class SignInRequest {

  constructor(
    public username: string="",
    public password: string=""
  ) {}

  toString(): string {
    return JSON.stringify(this)
  }
}