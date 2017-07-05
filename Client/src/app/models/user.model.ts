export class User {

  constructor(
    public username: string="",
    public firstName: string="",
    public lastName: string="",
    public email: string="",
    public birthDate="",
    public gender=""
  ) {}

  toString(): string {
    return JSON.stringify(this)
  }
}
