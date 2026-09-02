export interface UpdateSpaceRequest {
  name: string;
  location: string;
  description?: string | null;
  capacity: number;
  pricePerHour: number;
}
