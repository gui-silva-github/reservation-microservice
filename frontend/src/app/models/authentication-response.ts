export interface AuthenticationResponse {
  userId: string;
  name: string;
  email: string;
  role: string;
  token: string;
  success: boolean;
}
