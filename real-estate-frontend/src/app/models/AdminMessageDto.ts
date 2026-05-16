export interface AdminMessageListDto {
  messageId: number;
  type: string;
  fullName: string;
  subject: string;
  createdAt: string;
}

export interface AdminMessageDetailsDto {
  messageId: number;
  type: string;
  fullName: string;
  email: string;
  phone: string;
  subject: string;
  messageBody: string;
  createdAt: string;
}
