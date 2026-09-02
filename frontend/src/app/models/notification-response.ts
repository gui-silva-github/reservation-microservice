export interface NotificationResponse {
  id: string;
  userId: string;
  message: string;
  type: string;
  sentAt: string;
  read: boolean;
}
