export interface CreateSpaceRequest {
  name: string;
  location: string;
  description?: string | null;
  capacity: number;
  pricePerHour: number;
}
