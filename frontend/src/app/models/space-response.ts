export interface SpaceResponse {
  id: string;
  name: string;
  location: string;
  description: string | null;
  capacity: number;
  pricePerHour: number;
  isActive: boolean;
}
