import { Component, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { SpaceResponse } from '../../models/space-response';
import { SpacesService } from '../../services/spaces.service';
import { getApiErrorMessage } from '../../utils/api-error.util';

export interface DeleteProductDialogData {
  space: SpaceResponse;
}

@Component({
  selector: 'app-delete-product-dialog',
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, CurrencyPipe],
  templateUrl: './delete-product-dialog.html',
  styleUrl: './delete-product-dialog.css',
})
export class DeleteProductDialog {
  private readonly dialogRef = inject(MatDialogRef<DeleteProductDialog, boolean>);
  private readonly spacesService = inject(SpacesService);
  readonly data = inject<DeleteProductDialogData>(MAT_DIALOG_DATA);

  isDeleting = false;
  errorMessage = '';

  cancel(): void {
    this.dialogRef.close(false);
  }

  confirm(): void {
    this.isDeleting = true;
    this.errorMessage = '';

    this.spacesService.deleteSpace(this.data.space.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.isDeleting = false;
        this.errorMessage = getApiErrorMessage(error, 'Erro ao excluir o espaço. Tente novamente.');
      },
    });
  }
}
